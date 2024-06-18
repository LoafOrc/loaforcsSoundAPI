using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Behaviours;
using loaforcsSoundAPI.Data;
using loaforcsSoundAPI.LethalCompany;
using loaforcsSoundAPI.Providers.Conditions;
using loaforcsSoundAPI.Providers.Formats;
using loaforcsSoundAPI.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI {
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class SoundPlugin : BaseUnityPlugin {
        public static SoundPlugin Instance { get; private set; }
        internal static LoafLogger logger { get; private set; }

        internal static List<SoundPack> SoundPacks = new List<SoundPack>();
        
        [Obsolete]
        internal new static ManualLogSource Logger => logger;
        
        internal JoinableThreadPool nonstartupThreadPool;
        
        private void Awake() {
            logger = new LoafLogger(MyPluginInfo.PLUGIN_GUID);
            BepInEx.Logging.Logger.Sources.Add(logger);
            Instance = this;

            logger.LogInfo("Patching...");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

            logger.LogInfo("Setting up config...");
            new SoundPluginConfig(Config);
            logger.LogInfo("Logging Level at: " + SoundPluginConfig.LOGGING_LEVEL.Value);

            logger.LogInfo("Searching for soundpacks...");
            string[] subdirectories = Directory.GetDirectories(Paths.PluginPath);

            logger.LogInfo("Beginning Bindings");
            logger.LogInfo("Bindings => General => Audio Formats");
            SoundAPI.RegisterAudioFormatProvider(".mp3", new Mp3AudioFormat());
            SoundAPI.RegisterAudioFormatProvider(".ogg", new OggAudioFormat());
            SoundAPI.RegisterAudioFormatProvider(".wav", new WavAudioFormat());

            logger.LogInfo("Bindings => General => Conditions");
            SoundAPI.RegisterConditionProvider("config", new ConfigCondition());
            SoundAPI.RegisterConditionProvider("mod_installed", new ModInstalledConditionProvider());
            SoundAPI.RegisterConditionProvider("and", new AndCondition());
            SoundAPI.RegisterConditionProvider("not", new NotCondition());
            SoundAPI.RegisterConditionProvider("or", new OrCondition());


            LethalCompanyBindings.Bind();

            foreach (string subdirectory in subdirectories) {
                // Combine the current subdirectory with the file name
                foreach (string subsubdirectory in Directory.GetDirectories(subdirectory)) {
                    string subfilePath = Path.Combine(subsubdirectory, "sound_pack.json");
                    logger.LogDebug(subsubdirectory);

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

            logger.LogInfo("Starting up JoinableThreadPool.");

            if (SoundPluginConfig.ENABLE_MULTITHREADING.Value)
                nonstartupThreadPool = new JoinableThreadPool(SoundPluginConfig.THREADPOOL_MAX_THREADS.Value);
            
            foreach(SoundPack pack in SoundPacks) {
                pack.QueueLoad(nonstartupThreadPool);
            }
            
            if(nonstartupThreadPool != null) nonstartupThreadPool.Start();

            logger.LogInfo("Registering onSceneLoaded");

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode __) => {
                logger.LogLosingIt("NEW SCENE LOADED! Scanning for inital playOnAwake sounds...");
                logger.LogLosingIt($"scene.name: {scene.name}");
                
                foreach(AudioSource source in FindObjectsOfType<AudioSource>(true)) {
                    if(source.gameObject.scene != scene) continue; // already processed

                    if(source.playOnAwake) {
                        logger.LogLosingIt($"source: {source}");
                        logger.LogLosingIt($"source.gameObject: {source.gameObject}");
                        logger.LogLosingIt($"source.clip: {source.clip == null}");
                        logger.LogLosingIt($"{source.gameObject.name}:"+(source.clip == null? "null" : source.clip.name)+" calling Stop() because its play on awake.");
                        source.Stop();
                    }

                    AudioSourceReplaceHelper ext = source.gameObject.AddComponent<AudioSourceReplaceHelper>();
                    ext.source = source;
                }
            };
            
            logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID}:{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }
        
        internal void EnsureSoundsAreLoaded() {
            if(!SoundPluginConfig.ENABLE_MULTITHREADING.Value) return; // if multithreading is disabled, sounds have already been loaeded.
            
            logger.LogInfo("Ensuring all sounds are loaded...");
            nonstartupThreadPool.Join();
            logger.LogInfo("it's all good :3");
        }
    }
}