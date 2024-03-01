﻿using BepInEx;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using loaforcsSoundAPI.Providers.Variables;
using loaforcsSoundAPI.LethalCompany;

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

            logger.LogInfo("Beginning Bindings");
            logger.LogInfo("Bindings => General => Audio Formats");
            SoundReplacementAPI.RegisterAudioFormatProvider(".mp3", new Mp3AudioFormat());
            SoundReplacementAPI.RegisterAudioFormatProvider(".ogg", new OggAudioFormat());
            SoundReplacementAPI.RegisterAudioFormatProvider(".wav", new WavAudioFormat());

            logger.LogInfo("Bindings => General => Random Generators");
            SoundReplacementAPI.RegisterRandomProvider("pure", new PureRandomProvider());
            SoundReplacementAPI.RegisterRandomProvider("determinstic", new DeterminsticRandomProvider());

            logger.LogInfo("Bindings => General => Conditions");
            SoundReplacementAPI.RegisterConditionProvider("config", new ConfigCondition());
            SoundReplacementAPI.RegisterConditionProvider("equal", new EqualsCondition());
            SoundReplacementAPI.RegisterConditionProvider("greater_than", new GreaterThanCondition());
            SoundReplacementAPI.RegisterConditionProvider("greater_than_or_equal", new GreaterThanOrEqualCondition());
            SoundReplacementAPI.RegisterConditionProvider("mod_installed", new ModInstalledConditionProvider());

            logger.LogInfo("Bindings => General => Variables");
            SoundReplacementAPI.RegisterVariableProvider("config", new ConfigVariableProvider());

            LethalCompanyBindings.Bind();

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

            /*
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.potatoepet.AdvancedCompany")) {
                logger.LogWarning("========================");
                logger.LogWarning("JUST A HEADS UP!");
                logger.LogWarning("----------------");
                logger.LogWarning("You have AdvancedCompany installed,");
                logger.LogWarning("AdvancedCompany currently causes incompatibilites with loaforcsSoundAPI");
                logger.LogWarning("For more info go to this github issue:");
                logger.LogWarning("https://github.com/FluffyFishGames/AdvancedCompany/issues/130");
                logger.LogWarning("========================");
            }
            */

            logger.LogInfo("Starting second thread...");
            Thread nonStartup = new Thread(new ThreadStart(LoadNonStartupReplacements));
            nonStartup.Start();
            if (!SoundPluginConfig.ENABLE_MULTITHREADING.Value)
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
