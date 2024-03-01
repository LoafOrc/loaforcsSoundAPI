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
        internal RandomProvider Random { get; private set; }
        internal JObject RandomSettings { get; private set; }

        internal bool UpdateEveryFrame { get; private set; } = false;

        public SoundReplaceGroup(SoundPack pack, JObject data) {
            this.pack = pack;
            foreach(JObject replacer in data["replacements"]) {
                new SoundReplacementCollection(this, replacer);
            }

            if(data.ContainsKey("randomness")) {
                RandomSettings = data["randomness"] as JObject;
                Random = SoundReplacementAPI.RandomProviders[(string)RandomSettings["type"]];
            } else {
                Random = SoundReplacementAPI.RandomProviders["pure"];
            }

            if(data.ContainsKey("condition")) {
                Setup(this, data["condition"] as JObject);
            }

            if(data.ContainsKey("update_every_frame")) {
                UpdateEveryFrame = (bool)data["update_every_frame"];
            }
        }
    }
}
