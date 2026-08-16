using System;
using System.Collections.Generic;
using System.Linq;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.AudioClipLoading;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class SoundReplacementGroup : Conditional, IValidatable {
	[JsonConstructor]
	internal SoundReplacementGroup() { }

	public SoundReplacementGroup(SoundReplacementCollection parent, List<string> matches) {
		Parent = parent;
		Matches = matches;

		if(SoundPackDataHandler.LoadedPacks.Contains(parent.Pack)) {
			throw new InvalidOperationException("SoundPack has already been registered, trying to add a new SoundReplacementGroup does not work!");
		}

		parent.AddSoundReplacementGroup(this);
	}

	internal void AddSoundReplacement(SoundInstance sound) {
		Sounds.Add(sound);
	}

	[field: NonSerialized]
	public SoundReplacementCollection Parent { get; internal set; }
	public int Priority { get; private set; }

	public List<string> Matches { get; private set; } = [];
	public List<SoundInstance> Sounds { get; private set; } = [];

	public bool UpdateEveryFrame { get; internal set; }
	public float? Volume { get; private set; }

	public override void OnRegistered() {
		base.OnRegistered();
		foreach(SoundInstance sound in Sounds) {
			sound.OnRegistered();
		}

		// Imply "*:object:clip" from "object:clip"
		List<string> corrected = Matches.Select(match => match.Split(":").Length == 2 ? $"*:{match}" : match).ToList();

		Matches.Clear();
		Matches.AddRange(corrected);
	}

	internal void QueueSounds(IAudioClipLoader audioClipLoader, SoundPackLoadPipeline.SkippedResults results = null, HashSet<SoundInstance> sharedSounds = null, Dictionary<string, SoundInstance> uniqueSounds = null) {
		foreach(SoundInstance sound in Sounds) {
			if(sound.ShouldSkip()) {
				if(results != null) results.Sounds++;
				continue;
			}

			if(uniqueSounds?.TryAdd(sound.FullPath, sound) == false) {
				sharedSounds?.Add(sound);
				if(results != null) results.Shared++;
				continue;
			}

			audioClipLoader.Queue(sound);
		}
	}

	internal void DestroySounds() {
		foreach(SoundInstance sound in Sounds) {
			if(sound.Clip) Object.Destroy(sound.Clip);
		}
	}

	public override List<IValidatable.ValidationResult> Validate() {
		List<IValidatable.ValidationResult> results = base.Validate();

		foreach(string match in Matches.ToList()) {
			if(string.IsNullOrEmpty(match)) {
				results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, "Match string can not be empty!"));
				continue;
			}

			if(match.StartsWith("#")) {
				Matches.Remove(match);

				if(SoundPackLoadPipeline.mappings.TryGetValue(match[1..], out List<string> mappedStrings)) {
					Matches.AddRange(mappedStrings);
				} else {
					results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Mapping: '{match}' has not been found. If it's part of a soft dependency, make sure to use a 'mod_installed' condition with 'constant' enabled."));
				}
			}

			string[] processed = match.Split(":");

			if(processed.Length == 1) {
				// could maybe swap to a warning? but like it won't work at all with only one part to the match string so i think it should be a fail.
				results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"'{match}' is not valid! If you mean to match to all Audio clips with this name you must explicitly do '*:{match}'."));
			}

			if(processed.Length > 3) {
				results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.WARN, $"'{match}' has more than 3 parts! SoundAPI will handle this as '{match[0]}:{match[1]}:{match[2]}', discarding the rest!"));
			}
		}

		foreach(SoundInstance sound in Sounds) {
			sound.Parent = this; // !!! - Setting data while doing validation. If this ever breaks it's here!
			results.AddRange(sound.Validate());
		}

		if(Volume.HasValue) {
			Volume = Math.Clamp(Volume.Value, 0.0f, 1.0f);
		}

		return results;
	}

	public override SoundPack Pack {
		get => Parent.Pack;
		set {
			if(Parent.Pack != null) throw new InvalidOperationException("Pack has already been set.");
			Parent.Pack = value;
		}
	}

	public override string ToString() {
		string matches = string.Join(", ", Matches);
		string sounds = string.Join(", ", Sounds);
		return $"Parent: {Parent}\nMatches: {matches}\nSounds: {sounds}";
	}
}