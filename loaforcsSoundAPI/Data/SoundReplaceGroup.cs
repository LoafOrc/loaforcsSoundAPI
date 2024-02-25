using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace loaforcsSoundAPI.Data {
    public class SoundReplaceGroup {

        internal static Dictionary<string, List<SoundMatchString>> GlobalSoundReplacements = new Dictionary<string, List<SoundMatchString>>();
        internal Dictionary<SoundMatchString, List<SoundReplacement>> SoundReplacements = new Dictionary<SoundMatchString, List<SoundReplacement>>();

        public SoundPack pack { get; private set; }
        internal RandomProvider Random { get; private set; }
        internal JObject RandomSettings { get; private set; }
        internal JObject ConditionSettings { get; private set; }

        ConditionProvider GroupCondition = null;

        internal SoundReplaceGroup(SoundPack pack, JObject data) {
            this.pack = pack;
            foreach(JObject replacer in data["replacements"]) {
                List<SoundReplacement> replacements = new List<SoundReplacement>();
                foreach (JObject sound in replacer["sounds"]) {
                    CreateSoundReplacement(pack, replacements, sound);
                }

                List<SoundMatchString> matchStrings = new List<SoundMatchString>();
                if (replacer["matches"].GetType() == typeof(JValue)) {
                    matchStrings.Add(new SoundMatchString(this, (string)(replacer["matches"] as JValue).Value));
                } else {
                    foreach(string matchString in replacer["matches"]) {
                        matchStrings.Add(new SoundMatchString(this, matchString));
                    }
                }

                foreach(SoundMatchString matchString in matchStrings) {
                    List<SoundMatchString> existing = GlobalSoundReplacements.GetValueOrDefault(matchString.AudioName, new List<SoundMatchString>());
                    existing.Add(matchString);
                    if (existing.Count == 1) GlobalSoundReplacements.Add(matchString.AudioName, existing);
                    else GlobalSoundReplacements[matchString.AudioName] = existing;
                    SoundReplacements[matchString] = replacements;
                }
            }

            if(data.ContainsKey("randomness")) {
                RandomSettings = data["randomness"] as JObject;
                Random = SoundReplacementAPI.RandomProviders[(string)RandomSettings["type"]];
            } else {
                Random = SoundReplacementAPI.RandomProviders["pure"];
            }

            if(data.ContainsKey("condition")) {
                ConditionSettings = data["condition"] as JObject;
                GroupCondition = SoundReplacementAPI.ConditionProviders[(string)ConditionSettings["type"]];
            }
        }

        public bool TestCondition() {
            if (GroupCondition == null) return true;

            return GroupCondition.Evaluate(this, ConditionSettings);
        }

        private static void CreateSoundReplacement(SoundPack pack, List<SoundReplacement> replacements, JObject sound) {
            SoundReplacement replacement = new SoundReplacement();

            replacement.SoundPath = (string)sound["sound"];
            replacement.Weight = sound.GetValueOrDefault("weight", 1);

            SoundLoader.GetAudioClip(pack.PackPath, Path.GetDirectoryName(replacement.SoundPath), Path.GetFileName(replacement.SoundPath), out AudioClip clip);
            if (clip == null) {
                SoundPlugin.logger.LogError("Failed to get audio clip, check above more detailed error");
            } else {
                replacement.Clip = clip;
                replacements.Add(replacement);
            }
        }
    }
}
