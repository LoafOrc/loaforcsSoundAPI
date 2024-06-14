using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Behaviours;
using loaforcsSoundAPI.Data;
using loaforcsSoundAPI.LethalCompany;
using loaforcsSoundAPI.Providers.Conditions;
using loaforcsSoundAPI.Providers.Formats;
using loaforcsSoundAPI.Providers.Random;
using loaforcsSoundAPI.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI {
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class SoundPlugin : BaseUnityPlugin {
        public static SoundPlugin Instance { get; private set; }
        internal static ManualLogSource logger { get; private set; }

        internal static List<string> UniqueSounds;

        internal static List<SoundPack> SoundPacks = new List<SoundPack>();

        internal JoinableThreadPool nonstartupThreadPool;

        private void Awake() {
            logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
            Instance = this;

            logger.LogInfo("Patching...");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

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
            SoundReplacementAPI.RegisterRandomProvider("deterministic", new DeterminsticRandomProvider());

            logger.LogInfo("Bindings => General => Conditions");
            SoundReplacementAPI.RegisterConditionProvider("config", new ConfigCondition());
            SoundReplacementAPI.RegisterConditionProvider("mod_installed", new ModInstalledConditionProvider());
            SoundReplacementAPI.RegisterConditionProvider("and", new AndCondition());
            SoundReplacementAPI.RegisterConditionProvider("not", new NotCondition());
            SoundReplacementAPI.RegisterConditionProvider("or", new OrCondition());


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
            nonstartupThreadPool = new JoinableThreadPool(SoundPluginConfig.THREADPOOL_MAX_THREADS.Value);

            foreach(SoundPack pack in SoundPacks) {
                pack.QueueNonStartupOnThreadPool(nonstartupThreadPool);
            }

            nonstartupThreadPool.Start();
            if(!SoundPluginConfig.ENABLE_MULTITHREADING.Value) {
                logger.LogInfo("Multithreading is disabled :(, joining the thread pool and blocking the main thread.");
                nonstartupThreadPool.Join();
            }

            logger.LogInfo("Registering onSceneLoaded");

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode __) => {
                foreach(AudioSource source in FindObjectsOfType<AudioSource>(true)) {
                    if(source.gameObject.scene != scene) continue; // already processed


                    if(source.playOnAwake) {
                        source.Stop();
                    }

                    AudioSourceReplaceHelper ext = source.gameObject.AddComponent<AudioSourceReplaceHelper>();
                    ext.source = source;
                }
            };

            logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID}:{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        void OnDisable() {
            nonstartupThreadPool.Join();
        }
    }
}
