using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.Core.JSON;
using loaforcsSoundAPI.Core.Util;
using loaforcsSoundAPI.Reporting;
using loaforcsSoundAPI.SoundPacks.AudioClipLoading;
using loaforcsSoundAPI.SoundPacks.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace loaforcsSoundAPI.SoundPacks;

static class SoundPackLoadPipeline {
	// todo: maybe remove
	internal static event Action OnFinishedPipeline = delegate { };
	internal static Dictionary<string, List<string>> mappings = [ ];

	internal class SkippedResults {
		public int Collections;
		public int Groups;
		public int Sounds;
		public int Shared;
	}

	internal static async Task StartPipeline() {
		loaforcsSoundAPI.Logger.LogInfo("Starting Sound-pack loading pipeline");

		Stopwatch completeLoadingTimer = Stopwatch.StartNew();
		Stopwatch timer = Stopwatch.StartNew();

		// Step 1: Find and load packs
		List<SoundPack> packs = FindAndLoadPacks();
		loaforcsSoundAPI.Logger.LogInfo($"(Step 1) Loading Sound-pack definitions took {timer.ElapsedMilliseconds}ms");

		if(packs.Count == 0) loaforcsSoundAPI.Logger.LogWarning("No sound-packs were found to load! This can be ignorable if you're doing testing or using SoundAPI for another purpose, but if you expected sound-packs to load you may have set it up incorrectly.");

		timer.Restart();

		// Step 2: Load mappings
		// todo: function
		foreach(SoundPack pack in packs) {
			string mappingFile = Path.Combine(pack.PackFolder, "soundapi_mappings.json");
			if(File.Exists(mappingFile)) {
				Dictionary<string, List<string>> packDefinedMappings = JSONDataLoader.LoadFromFile<Dictionary<string, List<string>>>(mappingFile);

				foreach(KeyValuePair<string, List<string>> entry in packDefinedMappings) {
					if(mappings.ContainsKey(entry.Key)) {
						mappings[entry.Key].AddRange(entry.Value);
					} else {
						mappings[entry.Key] = entry.Value;
					}
				}
			}
		}

		loaforcsSoundAPI.Logger.LogInfo($"(Step 2) Loading Sound-pack mappings ('{mappings.Count}') took {timer.ElapsedMilliseconds}ms");
		timer.Restart();

		HashSet<SoundInstance> sharedSounds = []; // Sounds with already-loaded (duplicate) audio clips, to skip from loading and set later.
		Dictionary<string, SoundInstance> uniqueSounds = [];

		SkippedResults skippedStats = new SkippedResults();
		MultithreadedAudioClipLoader audioClipLoader = new MultithreadedAudioClipLoader();

		// Step 3: Load registries data and begin loading audio
		foreach(SoundPack pack in packs) {
			pack.Replacers.Load();

			if(PackLoadingConfig.EnableHotReloading) {
				pack.Replacers.EnableHotReload();
			}

			// Step 4: Enter foreach hell and fire async methods to begin UWR calls to load sounds.
			foreach(SoundReplacementCollection collection in pack.Replacers) {
				if(collection.ShouldSkip()) {
					skippedStats.Collections++;
					continue;
				}

				foreach(SoundReplacementGroup replacementGroup in collection.Replacements) {
					if(replacementGroup.ShouldSkip()) {
						skippedStats.Groups++;
						continue;
					}

					if(collection.UpdateEveryFrame) replacementGroup.UpdateEveryFrame = true;

					SoundPackDataHandler.AddReplacement(replacementGroup);
					replacementGroup.QueueSounds(audioClipLoader, skippedStats, sharedSounds, uniqueSounds);
				}
			}
		}

		#region boring other stuff

		int amountOfOperations = audioClipLoader.Count;
		loaforcsSoundAPI.Logger.LogInfo($"(Step 3) Skipped {skippedStats.Collections} collection(s), {skippedStats.Groups} replacement(s), {skippedStats.Sounds} sound(s)");
		loaforcsSoundAPI.Logger.LogInfo($"(Step 3) Loading sound replacement collections took {timer.ElapsedMilliseconds}ms");
		if(SoundReportHandler.CurrentReport != null) SoundReportHandler.CurrentReport.AudioClipsLoaded = amountOfOperations;
		loaforcsSoundAPI.Logger.LogInfo($"(Step 4) Started loading {amountOfOperations} audio file(s)");

		// Delay until splash screens are done and we can check in on the unity web requests
		loaforcsSoundAPI.Logger.LogInfo("Waiting for splash screens to complete to continue...");
		completeLoadingTimer.Stop();
		await Task.Yield();
		loaforcsSoundAPI.Logger.LogInfo("Splash screens done! Continuing pipeline");
		loaforcsSoundAPI.Logger.LogWarning("The game will freeze for a moment!");
		timer.Restart();
		completeLoadingTimer.Start(); // unpause

		// Step 5 & 6 are contained within here,
		// it would probably be nice to actually have a better way to order steps instead of hardcoding it in the log messages
		audioClipLoader.LoadAllBlocking();

		// Step 7: Restore shared clips.
		foreach(SoundInstance soundReplacement in sharedSounds) {
			if(uniqueSounds.TryGetValue(soundReplacement.FullPath, out SoundInstance sound)) {
				soundReplacement.Clip = sound.Clip;
			}
		}
		loaforcsSoundAPI.Logger.LogInfo($"(Step 7) Restored {skippedStats.Shared} already-loaded clips!");

		#endregion

		// Step 8: Fire event and final cleanup
		OnFinishedPipeline();
		mappings = null;

		loaforcsSoundAPI.Logger.LogInfo($"Entire load process took an effective {completeLoadingTimer.ElapsedMilliseconds}ms");
	}

	static List<SoundPack> FindAndLoadPacks(string entryPoint = "sound_pack.json") {
		Dictionary<string, SoundPack> packs = [ ];

		foreach(string file in Directory.GetFiles(Paths.PluginPath, entryPoint, SearchOption.AllDirectories)) {
			Debuggers.SoundReplacementLoader?.Log($"found entry point: '{file}'!");

			SoundPack pack = JSONDataLoader.LoadFromFile<SoundPack>(file);
			if(pack == null) continue; // json error

			if(packs.TryGetValue(pack.Name, out SoundPack existingPack)) {
				IValidatable.LogAndCheckValidationResult($"loading '{file}'", [
					new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"A sound-pack with name '{pack.Name}' was already loaded from '{LogFormats.FormatFilePath(Path.Combine(existingPack.PackFolder, "sound_pack.json"))}'. Skipping loading the duplicate!!")
				], pack.Logger);
				continue;
			}

			Debuggers.SoundReplacementLoader?.Log("json loaded, validating");
			List<IValidatable.ValidationResult> validationResult = pack.Validate();

			if(IValidatable.LogAndCheckValidationResult($"loading '{file}'", validationResult, pack.Logger)) {
				BepInPlugin metadata;

				if(PackLoadingConfig.MetadataSpoofing) {
					metadata = new BepInPlugin(pack.GUID, pack.Name, pack.Version ?? "1.0.0");
				} else {
					metadata = MetadataHelper.GetMetadata(typeof(loaforcsSoundAPI));
				}

				ConfigFile configFile = loaforcsSoundAPI.GenerateConfigFile(pack.GUID, metadata);
				configFile.SaveOnConfigSet = false; // dumb setting that's enabled by default
				pack.Bind(configFile);

				if(configFile.Count > 0) {
					configFile.Save();
				}

				packs[pack.Name] = pack;
				SoundPackDataHandler.AddLoadedPack(pack);
				Debuggers.SoundReplacementLoader?.Log($"pack folder: {pack.PackFolder}");
			}
		}

		Debuggers.SoundReplacementLoader?.Log($"loaded '{packs.Count}' packs.");
		return [.. packs.Values];
	}

	static LoadSoundOperation StartWebRequestOperation(SoundPack pack, SoundInstance sound, AudioType type) {
		string fullPath = Path.Combine(pack.PackFolder, "sounds", sound.Sound);

		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, type);

		return new LoadSoundOperation(sound, www.SendWebRequest());
	}
}