using AsmResolver.PE.DotNet.Metadata;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using loaforcsSoundAPI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace loaforcsSoundAPI.Data {
    public class SoundPack {
        private static List<SoundPack> LoadedSoundPacks = new List<SoundPack>();

        public string Name { get; private set; }
        public string PackPath { get; private set; }

        private List<SoundReplaceGroup> replaceGroups = new List<SoundReplaceGroup>();
        public IReadOnlyCollection<SoundReplaceGroup> ReplaceGroups { get { return replaceGroups.AsReadOnly(); } }

        string[] loadOnStartup;

        private Dictionary<string, dynamic> Config = new Dictionary<string, dynamic>();

        public T GetConfigOption<T>(string configID) {
            return ((ConfigEntry<T>)Config[configID]).Value;
        }

        public SoundPack(string folder) {
            SoundPlugin.logger.LogDebug($"Soundpack `{folder}` is being loaded.");
            Stopwatch loadTime = Stopwatch.StartNew();
            PackPath = Path.Combine(Paths.PluginPath, folder);
            string packData = File.ReadAllText(Path.Combine(PackPath, "sound_pack.json"));

            // Deserialize the JSON content into a dynamic object
            JObject jsonData = JsonConvert.DeserializeObject(packData) as JObject;
            Name = (string)jsonData["name"];
            if (string.IsNullOrEmpty(Name)) {
                SoundPlugin.logger.LogError($"`name` is missing or empty in `{folder}/sound_pack.json`");
                return;
            }

            if (!Directory.Exists(Path.Combine(PackPath, "replacers"))) {
                SoundPlugin.logger.LogInfo("You've succesfully made a Sound-Pack! Continue with the tutorial to learn how to begin replacing sounds.");
            } else {
                loadOnStartup = jsonData.GetValueOrDefault("load_on_startup", new string[0]).Select(name => { return name + ".json"; }).ToArray();

                if(loadOnStartup.Length == 0) {
                    SoundPlugin.logger.LogWarning($"No replacers were defined in `replacers` so every single replacer is being loaded on start-up. Consider adding some so that loaforcsSoundAPI can use multithreading.");
                    loadOnStartup = Directory.GetFiles(Path.Combine(PackPath, "replacers")).Select(Path.GetFileName).ToArray();
                }
                SoundPlugin.logger.LogInfo($"Loading: {string.Join(",", loadOnStartup)} on startup.");

                HandleReplacers(loadOnStartup);
            }

            if (jsonData.ContainsKey("config")) {
                ConfigFile configFile = new ConfigFile(Utility.CombinePaths(Paths.ConfigPath, "soundpack." + Name + ".cfg"), saveOnInit: false, MetadataHelper.GetMetadata(SoundPlugin.Instance));

                foreach (JProperty configDef in jsonData["config"]) {
                    JObject configSettings = configDef.Value as JObject;

                    if (!configSettings.ContainsKey("default")) {
                        SoundPlugin.logger.LogError($"`{configDef.Name} doesn't have a default value!");
                        continue;
                    }
                    if (!configSettings.ContainsKey("description")) {
                        SoundPlugin.logger.LogWarning($"`{configDef.Name} doesn't have a description, consider adding one!");
                    }

                    switch(configSettings["default"].Type) {
                        case JTokenType.Boolean:
                            Config.Add(configDef.Name, configFile.Bind(configDef.Name.Split(":")[0], configDef.Name.Split(":")[1], (bool)configSettings["default"]));
                            break;
                        default:
                            SoundPlugin.logger.LogError($"`{configSettings["default"].Type} configtype is currently unsupported!");
                            break;
                    }
                }
            }
            
            LoadedSoundPacks.Add(this);
            loadTime.Stop();
            SoundPlugin.logger.LogInfo($"Loaded {Name}(start-up) in {loadTime.ElapsedMilliseconds}ms.");
        }
        internal void LoadNonStartupGroups() {
            if (!Directory.Exists(Path.Combine(PackPath, "replacers"))) return;
            Stopwatch loadTime = Stopwatch.StartNew();
            string[] nonStartup = Directory.GetFiles(Path.Combine(PackPath, "replacers")).Where(replacer => { return !loadOnStartup.Contains(replacer); }).Select(Path.GetFileName).ToArray();
            HandleReplacers(nonStartup);
            loadTime.Stop();
            SoundPlugin.logger.LogInfo($"Loaded {Name}(non-startup) in {loadTime.ElapsedMilliseconds}ms.");
        }
        private void HandleReplacers(string[] replacers) {
            foreach(string replacer in replacers) {
                string filePath = Path.Combine(PackPath, "replacers", replacer);
                SoundPlugin.logger.LogDebug($"Parsing `{Path.GetFileName(filePath)}` as a sound replacer");
                string data = File.ReadAllText(filePath);
                JObject jsonData = JsonConvert.DeserializeObject(data) as JObject;
                new SoundReplaceGroup(this, jsonData);
            }
        }
    }
}
