using BepInEx;
using HarmonyLib;
using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.VFX.VisualEffectControlTrackController;

namespace loaforcsSoundAPI.Patches {
    [HarmonyPatch(typeof(AudioSource))]
    internal static class AudioSourcePatch {
        [HarmonyPrefix, 
            HarmonyPatch(nameof(AudioSource.PlayHelper), new Type[] { typeof(AudioSource), typeof(ulong) })
        ]
        internal static void Play(AudioSource source) {
            AudioClip replacement = GetReplacementClip(ProcessName(source, source.clip));
            if (replacement != null) {
                replacement.name = source.clip.name;
                source.clip = replacement;
            }
        }

        [HarmonyPrefix, HarmonyPatch(nameof(AudioSource.PlayOneShotHelper), new[] { typeof(AudioSource), typeof(AudioClip), typeof(float) })]
        internal static void PlayOneShotHelper(AudioSource source, ref AudioClip clip) {
            AudioClip replacement = GetReplacementClip(ProcessName(source, clip));
            if (replacement != null) {
                replacement.name = clip.name;
                clip = replacement;
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

        static string ProcessName(AudioSource source, AudioClip clip) {
            if (clip == null) return null;
            string filteredgameObjectName = ":" + source.gameObject.name.Replace("(Clone)", "").Trim();
            if(source.transform.parent != null) {
                filteredgameObjectName = source.transform.parent.name.Replace("(Clone)", "").Trim() + filteredgameObjectName;
            }
            
            /*
            if(!SoundPlugin.UniqueSounds.Contains($"{filteredgameObjectName}:{clip.name}")) {
                SoundPlugin.UniqueSounds.Add($"{filteredgameObjectName}:{clip.name}");
                using (StreamWriter sw = new StreamWriter(Path.Combine(Paths.PluginPath, "unique_sounds.txt"), true)) {
                    // Append the new line
                    foreach(var sound in SoundPlugin.UniqueSounds) { sw.WriteLine(sound); }
                }   
            }*/

            return $"{filteredgameObjectName}:{clip.name}";
        }

        static AudioClip GetReplacementClip(string name) {
            if(name == null) return null;
            SoundPlugin.logger.LogDebug("Getting replacement for: " + name);

            if (!SoundReplaceGroup.GlobalSoundReplacements.ContainsKey(name.Split(":")[2])) return null;

            SoundMatchString matchedString = null;
            foreach(SoundMatchString matchString in SoundReplaceGroup.GlobalSoundReplacements[name.Split(":")[2]]) {
                if(matchString.Matches(name)) {
                    matchedString = matchString; break;
                }
            }

            if (matchedString == null) return null;

            List<SoundReplacement> replacements = new List<SoundReplacement>(matchedString.Group.SoundReplacements[matchedString]);
            int totalWeight = 0;
            replacements.ForEach(replacement => totalWeight += replacement.Weight);

            int chosenWeight = UnityEngine.Random.Range(0, totalWeight);
            while (chosenWeight > 0) {
                chosenWeight -= replacements[0].Weight;
                replacements.RemoveAt(0);

                if (replacements.Count == 1) break;
            }
            return replacements[0].Clip;
        }
    }
}
