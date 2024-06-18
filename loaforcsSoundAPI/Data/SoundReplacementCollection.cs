using HarmonyLib;
using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace loaforcsSoundAPI.Data {
    internal class SoundReplacementCollection : Conditonal {
        internal readonly List<SoundReplacement> replacements = new List<SoundReplacement>();
        readonly List<string> matchers = new List<string>();
        internal readonly SoundReplaceGroup group;

        internal SoundReplacementCollection(SoundReplaceGroup group, JObject data) {
            this.group = group;
            if (data.TryGetValue("condition", out JToken value)) {
                Setup(group, value as JObject);
            }
            
            if (data["matches"].GetType() == typeof(JValue)) {
                RegisterWithMatch((string)data["matches"]);
            } else {
                foreach (string matchString in data["matches"]) {
                    RegisterWithMatch(matchString);
                }
            }

            foreach (JObject sound in data["sounds"]) {

                if (sound["sound"].Type == JTokenType.Null) {
                    SoundPlugin.logger.LogExtended("Adding null sound, will remove the sound when chosen");
                    replacements.Add(new SoundReplacement(group, sound) {
                        Weight = sound.GetValueOrDefault("weight", 1)
                    });
                }
                else {
                    SoundReplacement replacement = new(group, sound) {
                        Weight = sound.GetValueOrDefault("weight", 1)
                    };
                    SoundLoader.GetAudioClip(group.pack.PackPath, Path.GetDirectoryName((string)sound["sound"]), Path.GetFileName((string)sound["sound"]), out AudioClip clip);
                    if (clip == null) {
                        SoundPlugin.logger.LogError("Failed to get audio clip, check above more detailed error");
                    } else {
                        replacement.Clip = clip;
                        replacements.Add(replacement);
                    }
                }
            }

            
        }

        internal bool MatchesWith(string a) {
            foreach(string b in matchers) {
                if (SoundAPI.MatchStrings(a, b)) return true;
            }
            return false;
        }

        void RegisterWithMatch(string matchString) {
            string match = SoundAPI.FormatMatchString(matchString).Split(":")[2];
            List<SoundReplacementCollection> replacements = SoundAPI.SoundReplacements.GetValueOrDefault(match, new List<SoundReplacementCollection>());
            replacements.Add(this);
            matchers.Add(SoundAPI.FormatMatchString(matchString));
            SoundAPI.SoundReplacements[match] = replacements;
        }

        public override bool TestCondition() {
            return base.TestCondition() && group.TestCondition();
        }
    }

    internal class SoundReplacement : Conditonal {
        public int Weight = 1;

        public AudioClip Clip { get; set; }

        public SoundReplacement(SoundReplaceGroup group, JObject data) {
            if(data.ContainsKey("condition")) {
                Setup(group, data["condition"] as JObject);
            }
        }

        // public SoundReplaceGroup group { get; set; }
    }
}
