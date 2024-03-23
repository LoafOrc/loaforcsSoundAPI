using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI {
    internal class SoundPluginConfig {
        internal static ConfigEntry<bool> ENABLE_MULTITHREADING;
        internal static ConfigEntry<int> THREADPOOL_MAX_THREADS;
        internal SoundPluginConfig(ConfigFile config) {
            ENABLE_MULTITHREADING = config.Bind("SoundLoading", "Multithreading", true, 
                "Whether or not to use multithreading when loading a sound pack. If you haven't been told that you should disable multithreading, you probably don't need to!"+
                "\nThis setting may not be needed with the new 1.0.0 rewrite of multithreading."
                );

            THREADPOOL_MAX_THREADS = config.Bind("SoundLoading", "MaxThreadsInThreadPool", 32, 
                "Max amount of threads the loading thread pool can create at once.\n"+
                "Increasing this number will decrease loading of non-startup packs but may be unsupported by your CPU."
                );
        }
    }
}
