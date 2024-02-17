using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Patches {
    [HarmonyPatch(typeof(SoundManager))]
    internal static class SoundManagerPatch {
        [HarmonyPrefix(), HarmonyPatch(nameof(SoundManager.SetFearAudio))]
        internal static void pluhpluh(SoundManager __instance) {
            //SoundPlugin.logger.LogDebug("dgpaslfg");
        }
    }
}
