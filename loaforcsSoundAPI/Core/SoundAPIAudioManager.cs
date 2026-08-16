using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI.Core;

// todo: redo all of this lmao
class SoundAPIAudioManager : MonoBehaviour {
	internal static readonly Dictionary<AudioSource, AudioSourceAdditionalData> audioSourceData = [ ];
	internal static readonly HashSet<AudioSourceAdditionalData> liveAudioSourceData = [ ]; // this is a list of audio source additonal data's that should have their .Update() called
	internal static readonly Queue<AudioSourceAdditionalData> destroyedAudioSourceData = [ ];

	static SoundAPIAudioManager Instance;

	void Awake() {
		SceneManager.sceneLoaded += (_, _) => {
			if(!Instance) {
				SpawnManager();
			}
		};
	}

	internal static void SpawnManager() {
		loaforcsSoundAPI.Logger.LogInfo("Starting AudioManager.");
		GameObject manager = new GameObject("SoundAPI_AudioManager");
		DontDestroyOnLoad(manager);
		Instance = manager.AddComponent<SoundAPIAudioManager>();
	}

	// this seems icky but i do not care
	void Update() {
		Debuggers.UpdateEveryFrame?.Log($"sanity check: soundapi audio manager is running!");

		foreach(AudioSourceAdditionalData data in liveAudioSourceData) {
			data.Update();
			if(!data.Source) {
				destroyedAudioSourceData.Enqueue(data); // Queue destroyed AudioSource for cleanup.
			}
		}
		if(destroyedAudioSourceData.Count > 0) {
			while(destroyedAudioSourceData.TryDequeue(out AudioSourceAdditionalData data)) {
				Remove(data); // Run cleanup for destroyed AudioSource.
			}
		}
	}

	void OnDisable() {
		loaforcsSoundAPI.Logger.LogDebug("manager disabled");
	}

	void OnDestroy() {
		loaforcsSoundAPI.Logger.LogDebug("manager destroyed");
	}

	// This isn't particularly good, but because AudioSourceAdditionalData is not a behaviour it can't keep track of OnEnable, OnDisable itself
	// maybe putting this on a background thread that runs every few seconds would work but really i don't care too much, i want this project done
	// now with native backend this should only run on the harmony fallback backend
	internal static void RunCleanup() {
		loaforcsSoundAPI.Logger.LogDebug("cleaning up old audio source entries");
		foreach(AudioSourceAdditionalData data in audioSourceData.Values.ToArray()) {
			Remove(data);
		}
	}

	internal static void Remove(AudioSourceAdditionalData data) {
		liveAudioSourceData.Remove(data);
		audioSourceData.Remove(data.Source);
	}
}