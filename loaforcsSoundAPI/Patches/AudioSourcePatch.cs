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
        
        const int TOKEN_PARENT_NAME = 0;
        const int TOKEN_OBJECT_NAME = 1;
        const int TOKEN_CLIP_NAME = 2;
        
        [HarmonyPrefix,
         HarmonyPatch(nameof(AudioSource.Play), new Type[] { }),
         HarmonyPatch(nameof(AudioSource.Play), new Type[] { typeof(ulong) }),
             HarmonyPatch(nameof(AudioSource.Play), new Type[] { typeof(double) })
        ]
        static bool Play(AudioSource __instance) {
            if(TryReplaceAudio(__instance, __instance.clip, out AudioClip replacement)) {
                if (replacement == null) return false;
                __instance.clip = replacement;
            }

            if (AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) { helper._isPlaying = true;}

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
                    if(SoundPluginConfig.LOGGING_LEVEL.Value == SoundPluginConfig.LoggingLevel.IM_GOING_TO_LOSE_IT) SoundPlugin.logger.LogLosingIt(".Stop() updated ._isPlaying to false");
                }
            }
        }
        
        [HarmonyPrefix, HarmonyPatch(nameof(AudioSource.loop), MethodType.Setter)]
        static bool SetAudioSourceLooping(AudioSource __instance, bool value) {
            if(AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                if(SoundPluginConfig.LOGGING_LEVEL.Value == SoundPluginConfig.LoggingLevel.IM_GOING_TO_LOSE_IT) SoundPlugin.logger.LogLosingIt($"updating looping for {__instance.gameObject}, value: {value}");
                //SoundPlugin.logger.LogTraceback();
                helper.Loop = value;
                
                // only change our stuff
                return false;
            }

            return true;
        }
        
        [HarmonyPostfix, HarmonyPatch(nameof(AudioSource.loop), MethodType.Getter)]
        static void GetAudioSourceLooping(AudioSource __instance, ref bool __result) {
            if(AudioSourceReplaceHelper.helpers.TryGetValue(__instance, out AudioSourceReplaceHelper helper)) {
                if(SoundPluginConfig.LOGGING_LEVEL.Value == SoundPluginConfig.LoggingLevel.IM_GOING_TO_LOSE_IT) SoundPlugin.logger.LogLosingIt($"swapping out result of AudioSource.loop :3");
                __result = helper.Loop;
            }
        }
        
        internal static bool TryReplaceAudio(AudioSource __instance, AudioClip clip, out AudioClip replacement) {
            replacement = null;
            if(__instance.gameObject == null) {
                ("AudioSource has no GameObject!!");
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
            
            return true;
        }

        static string TrimGameObjectName(GameObject gameObject) {

            StringBuilder builder = new StringBuilder(gameObject.name);
            builder.Replace("(Clone)", "");
            
            string name = gameObject.name.Replace("(Clone)", "");
            for (int i = 0; i < 10; i++) {
                builder.Replace("(" + i + ")", "");
            }

            if(SoundPluginConfig.LOGGING_LEVEL.Value == SoundPluginConfig.LoggingLevel.IM_GOING_TO_LOSE_IT) SoundPlugin.logger.LogLosingIt($"trimmed `{gameObject.name}` to `{builder.ToString().Trim()}`");
            return builder.ToString().Trim();
        }
        
        static string[] ProcessName(AudioSource source, AudioClip clip) {
            if (clip == null) return null;
            if(source.transform.parent == null) {
                return ["*", TrimGameObjectName(source.gameObject), clip.name];
            }
            return [TrimGameObjectName(source.transform.parent.gameObject), TrimGameObjectName(source.gameObject), clip.name];
        }

        static bool TryGetReplacementClip(string[] name, out SoundReplacementCollection collection, out AudioClip clip) {
            collection = null;
            clip = null;
            if(name == null) return false;
            SoundPlugin.logger.LogExtended($"Getting replacement for: {string.Join(":",name)}");
            
            if (!SoundAPI.SoundReplacements.TryGetValue(name[TOKEN_CLIP_NAME], out List<SoundReplacementCollection> possibleCollections)) { return false; }

            possibleCollections = possibleCollections
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
