using HarmonyLib;

namespace loaforcsSoundAPI.Patches;

// because for some reason people really like to touch the HideGameManagerObject bepinex option :333333

[HarmonyPatch(typeof(BepInEx.Logging.Logger))]
static class LoggerPatch {
	[HarmonyPrefix, HarmonyPatch("LogMessage")]
	static void ReenableAndSaveConfigs(object data) {
		if (data is not "Chainloader startup complete") return; // this is icky, but patching Chainloader.Start just borks it lmao.
		SoundPlugin.Instance.EnsureSoundsAreLoaded();
	}
}