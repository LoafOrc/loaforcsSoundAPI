using BepInEx.Logging;

namespace loaforcsSoundAPI.Utils;

internal class LoafLogger(string sourceName) : ManualLogSource(sourceName) {
	internal void LogExtended(object message) {
		if (SoundPluginConfig.LOGGING_LEVEL.Value == SoundPluginConfig.LoggingLevel.EXTENDED ||
			SoundPluginConfig.LOGGING_LEVEL.Value == SoundPluginConfig.LoggingLevel.IM_GOING_TO_LOSE_IT) {
			LogDebug($"[EXTENDED] {message}");
		}
	}
	
	internal void LogLosingIt(object message) {
		if (SoundPluginConfig.LOGGING_LEVEL.Value == SoundPluginConfig.LoggingLevel.IM_GOING_TO_LOSE_IT) {
			LogDebug($"[IM_GOING_TO_LOSE_IT] {message}");
		}
	}

	internal void LogTraceback() {
		LogLosingIt(new System.Diagnostics.StackTrace().ToString());
	}
}