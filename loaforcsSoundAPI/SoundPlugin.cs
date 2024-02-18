using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System;
using System.Reflection;
using UnityEngine;
using loaforcsSoundAPI.Data;
using System.Collections.Generic;
using System.Linq;
using loaforcsSoundAPI.Behaviours;
using System.Threading;
using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Providers.Formats;
using loaforcsSoundAPI.Providers.Random;
using loaforcsSoundAPI.Providers.Conditions;

namespace loaforcsSoundAPI {
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class SoundPlugin : BaseUnityPlugin {
        public static SoundPlugin Instance { get; private set; }
        internal static ManualLogSource logger { get; private set; }

        internal static List<string> UniqueSounds;

        internal static List<SoundPack> SoundPacks = new List<SoundPack>();

        private void Awake() {
            logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
            Instance = this;

            logger.LogInfo("Patching...");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

            logger.LogInfo("Registering default fileformats");
            SoundReplacementAPI.RegisterAudioFormatProvider(".mp3", new Mp3AudioFormat());
            SoundReplacementAPI.RegisterAudioFormatProvider(".ogg", new OggAudioFormat());
            SoundReplacementAPI.RegisterAudioFormatProvider(".wav", new WavAudioFormat());
            logger.LogInfo("Registering default randomness providers");
            SoundReplacementAPI.RegisterRandomProvider("pure", new PureRandomProvider());
            SoundReplacementAPI.RegisterRandomProvider("determinstic", new DeterminsticRandomProvider());
            logger.LogInfo("Registering default condition providers");
            SoundReplacementAPI.RegisterConditionProvider("config", new ConfigCondition());

            /*
            // Read all lines from the file
            if(File.Exists(Path.Combine(Paths.PluginPath, "unique_sounds.txt")))
                UniqueSounds = File.ReadAllLines(Path.Combine(Paths.PluginPath, "unique_sounds.txt")).ToList();
            else
                UniqueSounds = new List<string>();
            */

            logger.LogInfo("Setting up config...");
            new SoundPluginConfig(Config);

            logger.LogInfo("Searching for soundpacks...");
            string[] subdirectories = Directory.GetDirectories(Paths.PluginPath);

            foreach (string subdirectory in subdirectories) {
                // Combine the current subdirectory with the file name
                foreach (string subsubdirectory in Directory.GetDirectories(subdirectory)) {
                    string subfilePath = Path.Combine(subsubdirectory, "sound_pack.json");
                    Logger.LogDebug(subsubdirectory);

                    // Check if the file exists
                    if (File.Exists(subfilePath)) {
                        SoundPacks.Add(new SoundPack(subsubdirectory));
                        break;
                    }
                }

                string filePath = Path.Combine(subdirectory, "sound_pack.json");

                // Check if the file exists
                if (File.Exists(filePath)) {
                    SoundPacks.Add(new SoundPack(subdirectory));
                }
            }

            logger.LogInfo("Starting second thread...");
            Thread nonStartup = new Thread(new ThreadStart(LoadNonStartupReplacements));
            nonStartup.Start();
            if(!SoundPluginConfig.ENABLE_MULTITHREADING.Value)
                nonStartup.Join();

            logger.LogInfo("Starting internal handler");
            GameObject soundHandler = new GameObject("SoundRepalceHandler");
            DontDestroyOnLoad(soundHandler);
            soundHandler.AddComponent<SoundReplacerHandler>();

            logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID}:{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        static void LoadNonStartupReplacements() {
            foreach(SoundPack pack in SoundPacks) {
                pack.LoadNonStartupGroups();
            }
        }
    }
}
