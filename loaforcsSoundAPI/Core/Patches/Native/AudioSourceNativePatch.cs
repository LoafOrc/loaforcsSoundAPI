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

		if(AudioSourceAdditionalData.TryGet(source, out AudioSourceAdditionalData data)) {
			Debuggers.NativeBackend?.Log($"AudioSource::RemoveFromManager() cleaned up an audio source");
			SoundAPIAudioManager.Remove(data);
		}

		_origRemoveFromManager(self);
	}

	static unsafe void Play(IntPtr self, double delay) {
		AudioSource source = NativeBackend.GetScriptingWrapper<AudioSource>(self);
		Debuggers.NativeBackend?.Log($"native detour source = {source} (gameobject: {source.gameObject.name})");

		AudioSourceAdditionalData data = AudioSourceAdditionalData.GetOrCreate(source);

		if(SoundReplacementHandler.TryReplaceAudio(data.Source, data.OriginalClip, out AudioClip replacement)) {
			if(!replacement) {
				return;
			}

			data.RealClip = replacement;
		}

		Debuggers.NativeBackend?.Log($"AudioSource::Play() with native detour. IntPtr self = {self}");
		_origPlay(self, delay);
	}
}