using System;
using System.Runtime.InteropServices;
using loaforcsSoundAPI.SoundPacks;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace loaforcsSoundAPI.Core.Patches.Native;

// some cleanup maybe?
static class AudioSourceNativePatch {
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	delegate void PlayDelegate(IntPtr self, double delay);

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	delegate void RemoveFromManagerDelegate(IntPtr self);

	static PlayDelegate _origPlay;
	static RemoveFromManagerDelegate _origRemoveFromManager;

	internal static void Init(NativeOffsets offsets) {
		_origPlay = NativeBackend.PatchNative<PlayDelegate>(offsets.AudioSource_Play, Play);
		if(offsets.AudioSource_RemoveFromManager.HasValue) {
			_origRemoveFromManager = NativeBackend.PatchNative<RemoveFromManagerDelegate>(offsets.AudioSource_RemoveFromManager.Value, PatchedRemoveFromManager);
		} else {
			loaforcsSoundAPI.Logger.LogWarning("No RemoveFromManager offset for this unity version, falling back.");
			SceneManager.sceneLoaded += (scene, _) => {
				// run goofy loop on everything
				SoundAPIAudioManager.RunCleanup();
			};
		}
	}

	static void PatchedRemoveFromManager(IntPtr self) {
		AudioSource source = NativeBackend.GetScriptingWrapper<AudioSource>(self);

		if(AudioSourceAdditionalData.TryGet(source, out AudioSourceAdditionalData data) && data.ReplacedWith?.UpdateEveryFrame != true) {
			Debuggers.NativeBackend?.Log($"AudioSource::RemoveFromManager() cleaned up an audio source: {source}");
			SoundAPIAudioManager.Remove(data);
		}

		_origRemoveFromManager(self);
	}

	static unsafe void Play(IntPtr self, double delay) {
		AudioSource source = NativeBackend.GetScriptingWrapper<AudioSource>(self);
		Debuggers.NativeBackend?.Log($"native detour source = {source} (gameobject: {source.gameObject.name})");

		AudioSourceAdditionalData data = AudioSourceAdditionalData.GetOrCreate(source);

		AudioSourcePlayEvent @event = new AudioSourcePlayEvent(source, data.OriginalClip, isOneShot: false);

		if(SoundReplacementHandler.TryReplaceAudio(in @event, out ReplacementResult? result)) {
			data.RealClip = result.Value.ReplacedClip;
			if(data.ReplacedWith?.Volume.HasValue == true) {
				source.volume = data.ReplacedWith.Volume.Value;
				Debuggers.AudioSourceAdditionalData?.Log($"Changed {source} (gameobject: {source.gameObject.name}) volume to: {source.volume}");
			}
		}

		Debuggers.NativeBackend?.Log($"AudioSource::Play() with native detour. IntPtr self = {self}");
		_origPlay(self, delay);
	}
}