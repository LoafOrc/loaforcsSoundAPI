using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.Core.Patches;
using loaforcsSoundAPI.Reporting;
using loaforcsSoundAPI.Reporting.Data;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace loaforcsSoundAPI.SoundPacks;

readonly struct ReplacementResult {
	public ReplacementResult(AudioClip replacement, SoundReplacementGroup group) {
		ReplacedClip = replacement;
		ReplacedWith = group;
		IsUpdateEveryFrame = group.UpdateEveryFrame;
	}

	public AudioClip ReplacedClip { get; }
	public SoundReplacementGroup ReplacedWith { get; }
	public bool IsUpdateEveryFrame { get; }
}

static class SoundReplacementHandler {
	const int TOKEN_PARENT_NAME = 0;
	const int TOKEN_OBJECT_NAME = 1;
	const int TOKEN_CLIP_NAME = 2;

	static readonly string[] _suffixesToRemove = [ "(Clone)" ];
	static readonly Dictionary<int, string> _cachedObjectNames = [ ];
	static readonly StringBuilder _builder = new StringBuilder();

	static string[] _workingName = [ ];

	internal static void Register() {
		SceneManager.sceneLoaded += (scene, _) => { _cachedObjectNames.Clear(); };
	}

	static bool ShouldBeReplaced(in AudioSourcePlayEvent @event) {
		if(!@event.Source.gameObject) {
			return false;
		}

		if(!@event.IsOneShot && @event.Data.ReplacedWith != null && @event.Data.ReplacedWith.UpdateEveryFrame) {
			return false;
		}

		return !@event.Data.DisableReplacing;
	}

	internal static bool TryReplaceAudio(in AudioSourcePlayEvent @event, [NotNullWhen(true)] out ReplacementResult? result) {
		result = null;

		if(!ShouldBeReplaced(in @event)) {
			return false;
		}

		string[] name = ArrayPool<string>.Shared.Rent(3);

		if(
			!TryProcessName(ref name, @event.Source, @event.Clip) ||
			!TryGetReplacementClip(name, out SoundReplacementGroup group, out AudioClip newClip, @event.Context ?? new DefaultConditionContext(@event.Source))
		) {
			ArrayPool<string>.Shared.Return(name);
			return false;
		}

		ArrayPool<string>.Shared.Return(name);

		newClip.name = group.Pack.Name + " " + @event.Clip.name;
		result = new ReplacementResult(newClip, group);

		// todo: this should probably not be handled here as with UEFOneShotFix the resulting AudioSource will be different than the one passed in the event.
		// it should be handled instead in the patches where
		if(!PatchConfig.UEFOneShotWorkaround || !result.Value.IsUpdateEveryFrame || !@event.IsOneShot) {
			@event.Data.ReplacedWith = group;
		}

		if(result.Value.IsUpdateEveryFrame) {
			Debuggers.UpdateEveryFrame?.Log($"swapped to a clip that uses update_every_frame !!! isOneShot = {@event.IsOneShot}");
		}

		return true;
	}

	[Obsolete]
	internal static bool TryReplaceAudio(AudioSource source, AudioClip clip, out AudioClip replacement) {
		if(TryReplaceAudio(new AudioSourcePlayEvent(source, clip, false), out ReplacementResult? result)) {
			replacement = result.Value.ReplacedClip;
			return true;
		}

		replacement = null;
		return false;
	}

	static string TrimObjectName(GameObject gameObject) {
		if(_cachedObjectNames.ContainsKey(gameObject.GetHashCode())) {
			return _cachedObjectNames[gameObject.GetHashCode()];
		}

		_builder.Clear();
		_builder.Append(gameObject.name);
		foreach(string suffix in _suffixesToRemove) {
			_builder.Replace(suffix, string.Empty);
		}

		// todo: maybe look at combining the two loops below? i dont think it'll mean much to care but might do something?

		for(int i = 0; i < _builder.Length; i++) {
			if(_builder[i] != '(') {
				continue;
			}

			int start = i;
			i++; // move to the digit part
			while(i < _builder.Length && char.IsDigit(_builder[i])) {
				i++;
			}

			if(i >= _builder.Length || _builder[i] != ')') {
				continue;
			}

			_builder.Remove(start, i - start + 1);
			i = start - 1;
		}

		// Handle trimming ending whitespace
		int endIndex = _builder.Length;
		for(; endIndex > 0; endIndex--) {
			if(_builder[endIndex - 1] != ' ') {
				break;
			}
		}

		_builder.Remove(endIndex, _builder.Length - endIndex);

		string finalName = _builder.ToString();
		_cachedObjectNames[gameObject.GetHashCode()] = finalName;

		return finalName;
	}

	static bool TryProcessName(ref string[] name, AudioSource source, AudioClip clip) {
		if(clip == null) {
			return false;
		}

		if(source.transform.parent == null) {
			name[TOKEN_PARENT_NAME] = "*";
		} else {
			name[TOKEN_PARENT_NAME] = TrimObjectName(source.transform.parent.gameObject);
		}

		name[TOKEN_OBJECT_NAME] = TrimObjectName(source.gameObject);
		name[TOKEN_CLIP_NAME] = clip.name;

		// probably should be handled with some delegate or something
		if(SoundReportHandler.CurrentReport != null) {
			string className;
			try {
				className = new StackTrace(true).GetFrame(5).GetMethod().DeclaringType.Name;
			} catch {
				className = "unknown caller";
			}

			SoundReport.PlayedSound playedSound = new SoundReport.PlayedSound($"{name[TOKEN_PARENT_NAME]}:{name[TOKEN_OBJECT_NAME]}:{name[TOKEN_CLIP_NAME]}", className, source.playOnAwake);

			if(SoundReportHandler.CurrentReport.PlayedSounds.FindIndex(playedSound.Equals) == -1)
			// only add new unique ones
			{
				SoundReportHandler.CurrentReport.PlayedSounds.Add(playedSound);
			}
		}

		Debuggers.MatchStrings?.Log($"{name[TOKEN_PARENT_NAME]}:{name[TOKEN_OBJECT_NAME]}:{name[TOKEN_CLIP_NAME]}");
		return true;
	}

	static bool TryGetReplacementClip(string[] name, out SoundReplacementGroup group, out AudioClip clip, IContext context) {
		group = null;
		clip = null;
		if(name == null) {
			return false;
		}

		Debuggers.SoundReplacementHandler?.Log($"beginning replacement attempt for {name[TOKEN_CLIP_NAME]}");

		if(!SoundPackDataHandler.SoundReplacements.TryGetValue(name[TOKEN_CLIP_NAME], out List<SoundReplacementGroup> possibleCollections)) {
			return false;
		}

		Debuggers.SoundReplacementHandler?.Log("sound dictionary hit");

		group = possibleCollections.Find(it => it.Parent.Evaluate(context) && it.Evaluate(context) && CheckGroupMatches(it, name));
		if(group == null) {
			return false;
		}

		Debuggers.SoundReplacementHandler?.Log("sound group that matches");

		List<SoundInstance> replacements = group.Sounds.FindAll(it => it.Clip && it.Evaluate(context));
		if(replacements.Count == 0) {
			return false;
		}

		Debuggers.SoundReplacementHandler?.Log("has valid sounds");

		int totalWeight = 0;
		replacements.ForEach(replacement => totalWeight += replacement.Weight);

		/*
		if (group.TryDequeue(out int seed)) {
			Random.InitState(seed); // i don't know the performance on InitState but it should maybe be fine?
		}
		*/

		int chosenWeight = Random.Range(0, totalWeight + 1);
		SoundInstance sound = null;
		foreach(SoundInstance t in replacements) {
			sound = t;
			chosenWeight -= sound.Weight;

			if(chosenWeight <= 0) {
				break;
			}
		}

		Debuggers.SoundReplacementHandler?.Log($"chosen sound: {sound}");
		if(sound.Clip == null) return true;

		clip = sound.Clip;
		Debuggers.SoundReplacementHandler?.Log("done, dumping stack trace!");
		Debuggers.SoundReplacementHandler?.Log(string.Join(", ", group.Matches));
		Debuggers.SoundReplacementHandler?.Log(clip.name);
		Debuggers.SoundReplacementHandler?.Log(new StackTrace(true).ToString().Trim());

		return true;
	}

	static bool CheckGroupMatches(SoundReplacementGroup group, string[] a) {
		foreach(string b in group.Matches) {
			if(MatchStrings(a, b)) {
				return true;
			}
		}

		return false;
	}

	static bool MatchStrings(string[] a, string b) {
		string[] expected = b.Split(":");
		if(expected[TOKEN_PARENT_NAME] != "*" && expected[TOKEN_PARENT_NAME] != a[TOKEN_PARENT_NAME]) {
			return false; // parent gameobject mismatch
		}

		if(expected[TOKEN_OBJECT_NAME] != "*" && expected[TOKEN_OBJECT_NAME] != a[TOKEN_OBJECT_NAME]) {
			return false; // gameobject mismatch
		}

		return a[TOKEN_CLIP_NAME] == expected[TOKEN_CLIP_NAME];
	}
}