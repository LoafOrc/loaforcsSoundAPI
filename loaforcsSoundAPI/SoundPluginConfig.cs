using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI {
    internal class SoundPluginConfig {
        internal static ConfigEntry<bool> ENABLE_MULTITHREADING;

        internal static ConfigEntry<bool> ADVANCED_TIMINGS;
        internal SoundPluginConfig(ConfigFile config) {
            ENABLE_MULTITHREADING = config.Bind("SoundLoading", "Multithreading", true, "Whether or not to use multithreading when loading a sound pack. If you haven't been told that you should disable multithreading, you probably don't need to!\nIf you are using a mod that skips past the startup sequence and straight into a game disabling this fixes a crash.");
        }
    }
}
