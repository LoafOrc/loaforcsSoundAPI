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
        internal static void Play(AudioSource __instance) {
            if(__instance.gameObject == null) {
                SoundPlugin.logger.LogWarning("AudioSource has no GameObject!!");
                return;
            }
            if(AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                if(helper.DisableReplacing) return;
            }
            AudioClip replacement = GetReplacementClip(ProcessName(__instance, __instance.clip), out SoundReplacementCollection collection);
            if (replacement != null) {
                replacement.name = __instance.clip.name;
                __instance.clip = replacement;
                if(helper == null) {
                    if (__instance.playOnAwake)
                        __instance.Stop();

                    helper = __instance.gameObject.AddComponent<AudioSourceReplaceHelper>();
                    helper.source = __instance;
                }
                
                helper.replacedWith = collection;
            }
        }

        [HarmonyPrefix, HarmonyPatch(nameof(AudioSource.PlayOneShot), new Type[] { typeof(AudioClip), typeof(float) })]
        internal static void PlayOneShot(AudioSource __instance, ref AudioClip clip) {
            if(__instance.gameObject == null) {
                SoundPlugin.logger.LogWarning("AudioSource has no GameObject!!");
                return;
            }
            if(AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                if(helper.DisableReplacing) return;
            }
            AudioClip replacement = GetReplacementClip(ProcessName(__instance, clip), out SoundReplacementCollection collection);
            if (replacement != null) {
                if (helper == null) {
                    if (__instance.playOnAwake)
                        __instance.Stop();

                    helper = __instance.gameObject.AddComponent<AudioSourceReplaceHelper>();
                    helper.source = __instance;
                }
                replacement.name = clip.name;
                clip = replacement;

                helper.replacedWith = collection;
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

        internal static AudioClip GetReplacementClip(string name, out SoundReplacementCollection collection) {
            collection = null;
            if(name == null) return null;
            SoundPlugin.logger.LogDebug($"Getting replacement for: {name} (doing top level search for {name.Split(":")[2]})");

            if (!SoundReplacementAPI.SoundReplacements.ContainsKey(name.Split(":")[2])) { return null; }

            List<SoundReplacementCollection> possibleCollections = SoundReplacementAPI.SoundReplacements[name.Split(":")[2]]
                .Where(x => x.MatchesWith(name))
                .Where(x => x.TestCondition())
                .ToList();

            if (possibleCollections.Count == 0) return null;
            if(possibleCollections.Count > 1) {
                //SoundPlugin.logger.LogWarning("Multiple soundpacks are replacing the same sounds, choosing a random one.");
            }
            collection = possibleCollections[UnityEngine.Random.Range(0, possibleCollections.Count)];
            List<SoundReplacement> replacements = collection.replacements.Where(x => x.TestCondition()).ToList();
            if(replacements.Count == 0) return null;

            int totalWeight = 0;
            replacements.ForEach(replacement => totalWeight += replacement.Weight);

            int chosenWeight = collection.group.Random.Range(collection.group, 0, totalWeight);
            int chosen = 0;
            while (chosenWeight > 0) {
                chosen = collection.group.Random.Range(collection.group, 0, replacements.Count);
                chosenWeight -= collection.group.Random.Range(collection.group, 1, replacements[chosen].Weight);
            }
            return replacements[chosen].Clip;
        }
    }
}
