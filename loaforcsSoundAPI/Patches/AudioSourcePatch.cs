using BepInEx;
using HarmonyLib;
using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Behaviours;
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
        static bool Play(AudioSource __instance) {
            if(TryReplaceAudio(__instance, __instance.clip, out AudioClip replacement)) {
                if (replacement == null) return false;
                __instance.clip = replacement;
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(nameof(AudioSource.PlayOneShot), new Type[] { typeof(AudioClip), typeof(float) })]
        static bool PlayOneShot(AudioSource __instance, ref AudioClip clip) {
            if (TryReplaceAudio(__instance, clip, out AudioClip replacement)) {
                if (replacement == null) return false;
                clip = replacement;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(AudioSource.Stop), [typeof(bool)])]
        static void UpdateIsPlayingForHelper(AudioSource __instance) {
            if (AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                if (helper._isPlaying) {
                    helper._isPlaying = false;
                    SoundPlugin.logger.LogLosingIt(".Stop() updated ._isPlaying to false");
                }
            }
        }
        
        [HarmonyPrefix, HarmonyPatch(nameof(AudioSource.loop), MethodType.Setter)]
        static bool SetAudioSourceLooping(AudioSource __instance, bool value) {
            if(AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                SoundPlugin.logger.LogLosingIt($"updating looping for {TrimGameObjectName(__instance.gameObject)}, value: {value}");
                helper.Loop = value;
                
                // only change our stuff
                return false;
            }

            return true;
        }
        
        [HarmonyPostfix, HarmonyPatch(nameof(AudioSource.loop), MethodType.Getter)]
        static void GetAudioSourceLooping(AudioSource __instance, ref bool __result) {
            if(AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                SoundPlugin.logger.LogLosingIt($"swapping out result of AudioSource.loop :3");
                __result = helper.Loop;
            }
        }
        
        internal static bool TryReplaceAudio(AudioSource __instance, AudioClip clip, out AudioClip replacement) {
            replacement = null;
            if(__instance.gameObject == null) {
                SoundPlugin.logger.LogWarning("AudioSource has no GameObject!!");
                return false;
            }
            if(AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                if(helper.DisableReplacing) return false;
            }
            if(!TryGetReplacementClip(ProcessName(__instance, clip), out SoundReplacementCollection collection, out AudioClip newClip)) return false;

            if (helper == null) {
                if (__instance.playOnAwake)
                    __instance.Stop();

                helper = __instance.gameObject.AddComponent<AudioSourceReplaceHelper>();
                helper.source = __instance;
            }
            newClip.name = clip.name;
            replacement = newClip;

            helper.replacedWith = collection;
            helper._isPlaying = true;
            
            return true;
        }

        static string TrimGameObjectName(GameObject gameObject) {
            
            string name = gameObject.name.Replace("(Clone)", "");
            for (int i = 0; i < 10; i++) {
                name = name.Replace("(" + i + ")", "");
            }

            SoundPlugin.logger.LogLosingIt($"trimmed `{gameObject.name}` to `{name.Trim()}`");
            return name.Trim();
        }

        static string ProcessName(AudioSource source, AudioClip clip) {
            if (clip == null) return null;
            string filteredgameObjectName = ":" + TrimGameObjectName(source.gameObject);
            if(source.transform.parent != null) {
                filteredgameObjectName = TrimGameObjectName(source.transform.parent.gameObject) + filteredgameObjectName;
            }

            return $"{filteredgameObjectName}:{clip.name}";
        }

        static bool TryGetReplacementClip(string name, out SoundReplacementCollection collection, out AudioClip clip) {
            collection = null;
            clip = null;
            if(name == null) return false;
            SoundPlugin.logger.LogExtended($"Getting replacement for: {name}");
            
            if (!SoundAPI.SoundReplacements.ContainsKey(name.Split(":")[2])) { return false; }

            List<SoundReplacementCollection> possibleCollections = SoundAPI.SoundReplacements[name.Split(":")[2]]
                .Where(x => x.MatchesWith(name))
                .Where(x => x.TestCondition())
                .ToList();

            if(possibleCollections.Count == 0) return false;
            
            collection = possibleCollections[UnityEngine.Random.Range(0, possibleCollections.Count)];
            List<SoundReplacement> replacements = collection.replacements.Where(x => x.TestCondition()).ToList();
            if(replacements.Count == 0) return false;

            int totalWeight = 0;
            replacements.ForEach(replacement => totalWeight += replacement.Weight);

            int chosenWeight = UnityEngine.Random.Range(0, totalWeight);
            int chosen = 0;
            while (chosenWeight > 0) {
                chosen = UnityEngine.Random.Range(0,  replacements.Count);
                chosenWeight -= UnityEngine.Random.Range(1, replacements[chosen].Weight);
            }
            clip = replacements[chosen].Clip;
            return true;
        }
    }
}
