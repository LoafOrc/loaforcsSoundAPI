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
            if (data.ContainsKey("condition")) {
                Setup(group, data["condition"] as JObject);
            }

            foreach (JObject sound in data["sounds"]) {
                SoundReplacement replacement = new(group, sound) {
                    SoundPath = (string)sound["sound"],
                    Weight = sound.GetValueOrDefault("weight", 1)
                };

                SoundLoader.GetAudioClip(group.pack.PackPath, Path.GetDirectoryName(replacement.SoundPath), Path.GetFileName(replacement.SoundPath), out AudioClip clip);
                if (clip == null) {
                    SoundPlugin.logger.LogError("Failed to get audio clip, check above more detailed error");
                } else {
                    replacement.Clip = clip;
                    replacements.Add(replacement);
                }
            }

            if (data["matches"].GetType() == typeof(JValue)) {
                RegisterWithMatch((string)data["matches"]);
            } else {
                foreach (string matchString in data["matches"]) {
                    RegisterWithMatch(matchString);
                }
            }
        }

        internal bool MatchesWith(string a) {
            foreach(string b in matchers) {
                if (SoundReplacementAPI.MatchStrings(a, b)) return true;
            }
            return false;
        }

        void RegisterWithMatch(string matchString) {
            string match = SoundReplacementAPI.FormatMatchString(matchString).Split(":")[2];
            List<SoundReplacementCollection> replacements = SoundReplacementAPI.SoundReplacements.GetValueOrDefault(match, new List<SoundReplacementCollection>());
            replacements.Add(this);
            matchers.Add(SoundReplacementAPI.FormatMatchString(matchString));
            SoundReplacementAPI.SoundReplacements[match] = replacements;
        }

        public override bool TestCondition() {
            return base.TestCondition() && group.TestCondition();
        }
    }

    internal class SoundReplacement : Conditonal {
        public string SoundPath { get; set; }
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
