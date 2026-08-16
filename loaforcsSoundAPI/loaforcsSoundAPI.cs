using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.Core.Patches;
using loaforcsSoundAPI.Core.Patches.Harmony;
using loaforcsSoundAPI.Core.Patches.Native;
using loaforcsSoundAPI.Patcher;
using loaforcsSoundAPI.Reporting;
using loaforcsSoundAPI.SoundPacks;
using UnityEngine;

namespace loaforcsSoundAPI;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
class loaforcsSoundAPI : BaseUnityPlugin {
	internal new static ManualLogSource Logger { get; private set; }

	void Awake() {
		Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
		Config.SaveOnConfigSet = false;

		Logger.LogInfo("Setting up config");
		Debuggers.Bind(Config);
		SoundReportHandler.Bind(Config);
		PatchConfig.Bind(Config);
		PackLoadingConfig.Bind(Config);

		// Subscribe to Action from preloader.
		LoaforcsSoundAPIPatcher.Instance.ChainloaderFinish += async () => await SoundPackLoadPipeline.StartPipeline();

		Logger.LogInfo("Running patches");
		Harmony harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

		// this is bad lmao, but the warning should only be if nativebackend is the preferred.
		if(PatchConfig.PreferredBackend == PatchConfig.Backend.HarmonyX) {
			Logger.LogInfo("Native backend is manually disabled.");
			HarmonyBackend.Init(harmony);
		} else if(NativeBackend.TryInit()) {
			Logger.LogInfo($"Native backend is supported on {Application.unityVersion}!");
		} else {
			Logger.LogWarning("Native backend failed, falling back to default harmony backend!");
			HarmonyBackend.Init(harmony);
		}

		Logger.LogInfo("Registering data");
		SoundAPI.RegisterAll(Assembly.GetExecutingAssembly());

		SoundAPIAudioManager.SpawnManager();

		SoundReplacementHandler.Register();
		Config.Save();

		Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} by loaforc has loaded :3");
	}

	internal static ConfigFile GenerateConfigFile(string name, BepInPlugin metadata) {
		return new ConfigFile(Utility.CombinePaths(Paths.ConfigPath, name + ".cfg"), false, metadata);
	}
}