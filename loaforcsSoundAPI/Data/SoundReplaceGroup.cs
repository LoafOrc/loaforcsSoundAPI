using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace loaforcsSoundAPI.Data {
    public class SoundReplaceGroup : Conditonal {

        public SoundPack pack { get; private set; }
        internal JObject RandomSettings { get; private set; }

        internal bool UpdateEveryFrame { get; private set; } = false;
        internal bool IgnoreLooping { get; private set; } = false;
        
        public SoundReplaceGroup(SoundPack pack, JObject data) {
            this.pack = pack;
            if(data.ContainsKey("condition")) {
                Setup(this, data["condition"] as JObject);
                
                if ((string)data["condition"]["type"] == "config" && SoundPluginConfig.SKIP_LOADING_UNUSED_SOUNDS.Value) {
                    if (!TestCondition()) {
                        SoundPlugin.logger.LogLosingIt("Skipping loading SoundReplaceGroup because the config is disabled..");
                        return;
                    }
                }
            }
            
            SoundPlugin.logger.LogLosingIt("Loading audio");
            foreach(JObject replacer in data["replacements"]) {
                new SoundReplacementCollection(this, replacer);
            }
            SoundPlugin.logger.LogLosingIt("Done loading audio");

            if(data.ContainsKey("update_every_frame")) {
                UpdateEveryFrame = (bool)data["update_every_frame"];
            }
            if(data.ContainsKey("ignore_looping")) {
                UpdateEveryFrame = (bool)data["ignore_looping"];
            }

            if (data.ContainsKey("randomnesss")) {
                SoundPlugin.logger.LogWarning($"Found deprecated value `randomness` for pack `{pack.Name}`");
            }
        }
    }
}
