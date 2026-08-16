using HarmonyLib;
using loaforcsSoundAPI.Core.Util.Extensions;
using loaforcsSoundAPI.SoundPacks;
using UnityEngine;

namespace loaforcsSoundAPI.Core.Patches;

[HarmonyPatch(typeof(AudioSource))]
static class AudioSourcePatch {
	internal static bool bypassSpoofing;

	// todo: this should maybe be supported in NativeBackend?
	[HarmonyPrefix]
	[HarmonyPatch(nameof(AudioSource.PlayOneShot), [typeof(AudioClip), typeof(float)])]
	static bool PlayOneShot(AudioSource __instance, ref AudioClip clip, float volumeScale) {
		if(!clip) {
			return true; // returning true here gives the default unity warning
		}

		AudioSourcePlayEvent @event = new AudioSourcePlayEvent(__instance, clip, isOneShot: true);

		if(SoundReplacementHandler.TryReplaceAudio(in @event, out ReplacementResult? result)) {
			if(result.Value.IsUpdateEveryFrame) {
				if(PatchConfig.UEFOneShotWorkaround) {
					GameObject cloneTarget = new GameObject($"UEFOneShotFix - {clip.name}");
					cloneTarget.transform.SetParent(__instance.transform, false);
					AudioSource clone = SoundAPI.CopyAudioSource(__instance, cloneTarget, AudioSourceCopyFlags.DontCopyPlayOnAwake | AudioSourceCopyFlags.DontCopySpatialize | AudioSourceCopyFlags.DontCopyLoop);
					AudioSourceAdditionalData data = AudioSourceAdditionalData.GetOrCreate(clone);
					SoundAPIAudioManager.liveAudioSourceData.Add(data); // Set UEF OneShot as a live AudioSource, for proper cleanup after it gets destroyed.
					clone.clip = result.Value.ReplacedClip;
					clone.volume *= volumeScale;
					data.ReplacedWith = result.Value.ReplacedWith; // setting replaced with here is important, see SoundReplacementHandler.ShouldBeReplaced (would have to handle DisableReplacing manually instead)
					data.VolumeScale = volumeScale;
					if(data.ReplacedWith?.Volume.HasValue == true) {
						clone.volume = data.ReplacedWith.Volume.Value * volumeScale;
						if(Debuggers.AudioSourceAdditionalData != null) {
							string volume = $"{clone.volume}";
							if(clone.volume != result.Value.ReplacedWith.Volume.Value) {
								volume += $" ({result.Value.ReplacedWith.Volume.Value} * {volumeScale})";
							}
							Debuggers.AudioSourceAdditionalData?.Log($"Changed {clone} (gameobject: {clone.name}) volume to: {volume})");
						}
					}
					clone.PlayThenDestroy(); // TODO: Object pooling instead of destroying.

					return false;
				}

				loaforcsSoundAPI.Logger.LogWarning("Replacing a PlayOneShot with a UpdateEveryFrame replacer, things are likely to break. Enable `Experiments -> UEFOneShotFix` to make this warning disappear (and help test it ig)");
			}

			clip = result.Value.ReplacedClip;
			if(result.Value.ReplacedWith?.Volume.HasValue == true) {
				__instance.volume = result.Value.ReplacedWith.Volume.Value * volumeScale;
				if(Debuggers.AudioSourceAdditionalData != null) {
					string volume = $"{__instance.volume}";
					if(__instance.volume != result.Value.ReplacedWith.Volume.Value) {
						volume += $" ({result.Value.ReplacedWith.Volume.Value} * {volumeScale})";
					}
					Debuggers.AudioSourceAdditionalData?.Log($"Changed {__instance} (gameobject: {__instance.name}) volume to: {volume})");
				}
			}
		}

		return true;
	}

	[HarmonyPatch(nameof(AudioSource.clip), MethodType.Setter)]
	[HarmonyPriority(Priority.Last)]
	[HarmonyPrefix]
	static void UpdateOriginalClip(AudioSource __instance, AudioClip value, bool __runOriginal) {
		if(!__runOriginal || bypassSpoofing) {
			return;
		}

		AudioSourceAdditionalData data = AudioSourceAdditionalData.GetOrCreate(__instance);
		data.OriginalClip = value;
		Debuggers.AudioClipSpoofing?.Log($"({__instance.gameObject.name}) updating original clip to: {value.name}");
	}

	[HarmonyPatch(nameof(AudioSource.clip), MethodType.Setter)]
	[HarmonyPrefix]
	static bool PreventClipRestartingWithSpoofed(AudioSource __instance, AudioClip value) {
		if(!PatchConfig.AudioClipSpoofing || bypassSpoofing) {
			return true;
		}

		/*
		 * Sometimes a game/mod (like REPO) will update AudioSource.clip frequently.
		 * In cases where SoundAPI will replace this clip it will cause the audio to consistently restart.
		 *
		 * This preforms the intended behaviour from the game/mod creator pov where the audio does not restart if they think they are setting it to the same thing
		 */
		AudioSourceAdditionalData data = AudioSourceAdditionalData.GetOrCreate(__instance);
		if(data.OriginalClip == value) {
			Debuggers.AudioClipSpoofing?.Log("prevented clip from restarting");
		}

		return data.OriginalClip != value;
	}

	[HarmonyPatch(nameof(AudioSource.clip), MethodType.Getter)]
	[HarmonyPostfix]
	static void SpoofAudioSourceClip(AudioSource __instance, ref AudioClip __result) {
		if(!PatchConfig.AudioClipSpoofing || bypassSpoofing) {
			return;
		}

		AudioSourceAdditionalData data = AudioSourceAdditionalData.GetOrCreate(__instance);
		__result = data.OriginalClip;
		Debuggers.AudioClipSpoofing?.Log($"({__instance.gameObject.name}) spoofing result to {((data.OriginalClip != null) ? data.OriginalClip.name : "null")}");
	}
}