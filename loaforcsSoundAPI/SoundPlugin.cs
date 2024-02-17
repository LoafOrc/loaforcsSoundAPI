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
using loaforcsSoundAPI.Formats;

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
            SoundReplacementAPI.RegisterAudioFormat(".mp3", new Mp3AudioFormat());
            SoundReplacementAPI.RegisterAudioFormat(".ogg", new OggAudioFormat());
            SoundReplacementAPI.RegisterAudioFormat(".wav", new WavAudioFormat());

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
