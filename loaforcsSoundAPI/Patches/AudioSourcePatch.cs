﻿using BepInEx;
using HarmonyLib;
using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace loaforcsSoundAPI.Patches {
    [HarmonyPatch(typeof(AudioSource))]
    internal static class AudioSourcePatch {
        [HarmonyPrefix, 
            HarmonyPatch(nameof(AudioSource.Play), new Type[] { }),
            HarmonyPatch(nameof(AudioSource.Play), new Type[] { typeof(ulong) })
        ]
        internal static void Play(AudioSource __instance) {
            AudioClip replacement = GetReplacementClip(ProcessName(__instance, __instance.clip));
            if (replacement != null) {
                replacement.name = __instance.clip.name;
                __instance.clip = replacement;
            }
        }

        [HarmonyPrefix, HarmonyPatch(nameof(AudioSource.PlayOneShot), new Type[] { typeof(AudioClip), typeof(float) })]
        internal static void PlayOneShot(AudioSource __instance, ref AudioClip clip) {
            AudioClip replacement = GetReplacementClip(ProcessName(__instance, clip));
            if (replacement != null) {
                replacement.name = clip.name;
                clip = replacement;
            }
        }

        internal static string TrimGameObjectName(GameObject gameObject) {
            string name = gameObject.name.Replace("(Clone)", "");
            for (int i = 0; i < 10; i++) {
                name = name.Replace("(" + i + ")", "");
            }

            return name.Trim();
        }

        internal static string ProcessName(AudioSource source, AudioClip clip) {
            if (clip == null) return null;
            string filteredgameObjectName = ":" + TrimGameObjectName(source.gameObject);
            if(source.transform.parent != null) {
                filteredgameObjectName = TrimGameObjectName(source.transform.parent.gameObject) + filteredgameObjectName;
            }

            return $"{filteredgameObjectName}:{clip.name}";
        }

        internal static AudioClip GetReplacementClip(string name) {
            if(name == null) return null;
            SoundPlugin.logger.LogDebug("Getting replacement for: " + name);

            if (!SoundReplaceGroup.GlobalSoundReplacements.ContainsKey(name.Split(":")[2])) return null;

            SoundMatchString matchedString = null;
            foreach(SoundMatchString matchString in SoundReplaceGroup.GlobalSoundReplacements[name.Split(":")[2]]) {
                if(matchString.Matches(name)) {
                    if (matchString.Group.TestCondition()) {
                        matchedString = matchString; 
                        break;
                    }
                }
            }

            if (matchedString == null) return null;

            List<SoundReplacement> replacements = new List<SoundReplacement>(matchedString.Group.SoundReplacements[matchedString]);
            int totalWeight = 0;
            replacements.ForEach(replacement => totalWeight += replacement.Weight);



            int chosenWeight = matchedString.Group.Random.Range(matchedString.Group, 0, totalWeight);
            SoundPlugin.logger.LogDebug($"totalWeight: {totalWeight}, chosenWeight: {chosenWeight}");
            int chosen = 0;
            while (chosenWeight > 0) {
                chosen = matchedString.Group.Random.Range(matchedString.Group, 0, replacements.Count);
                SoundPlugin.logger.LogDebug($"removing between 1 and {replacements[chosen].Weight}");
                chosenWeight -= matchedString.Group.Random.Range(matchedString.Group, 1, replacements[chosen].Weight);
                SoundPlugin.logger.LogDebug($"chosenWeight is now {chosenWeight}, chosen: {chosen}");
            }
            SoundPlugin.logger.LogDebug($"selected a clip");
            return replacements[chosen].Clip;
        }
    }
}
