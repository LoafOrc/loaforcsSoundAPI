using HarmonyLib;
using loaforcsSoundAPI.Behaviours;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace loaforcsSoundAPI.Patches;
[HarmonyPatch(typeof(GameObject))]
internal class GameObjectPatch {
    [HarmonyPostfix, HarmonyPatch(nameof(GameObject.AddComponent), new Type[] { typeof(Type) })]
    internal static void NewAudioSource(GameObject __instance, ref Component __result) {
        if (__result is not AudioSource) return;

        AudioSource source = __result as AudioSource;
        if (source.playOnAwake)
            source.Stop();

        AudioSourceReplaceHelper ext = __instance.AddComponent<AudioSourceReplaceHelper>();
        ext.source = source;
        SoundPlugin.logger.LogLosingIt("Handled AudioSource created via .AddComponent()");
    }
}
