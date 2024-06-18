using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI {
    internal class SoundPluginConfig {
        internal static ConfigEntry<bool> ENABLE_MULTITHREADING;
        internal static ConfigEntry<int> THREADPOOL_MAX_THREADS;

        internal static ConfigEntry<LoggingLevel> LOGGING_LEVEL;
        
        static ConfigEntry<int> CONFIG_VERSION;

        internal enum LoggingLevel {
            NORMAL,
            EXTENDED,
            IM_GOING_TO_LOSE_IT
        }
        
        internal SoundPluginConfig(ConfigFile config) {
            ENABLE_MULTITHREADING = config.Bind("SoundLoading", "Multithreading", true, 
                "Whether or not to use multithreading when loading a sound pack. If you haven't been told that you should disable multithreading, you probably don't need to!"+
                "\nThis setting may not be needed with the new 1.0.0 rewrite of multithreading."
                );
            
            THREADPOOL_MAX_THREADS = config.Bind("SoundLoading", "MaxThreadsInThreadPool", 4, 
                "Max amount of threads the loading thread pool can create at once.\n"+
                "Increasing this number will decrease loading of non-startup packs but may be unsupported by your CPU."
                );

            LOGGING_LEVEL = config.Bind("Logging", "LoggingLevel", LoggingLevel.NORMAL, "What level should sound api log at?");
            
            CONFIG_VERSION = config.Bind("INTERNAL_DO_NOT_TOUCH", "CONFIG_VERSION_DO_NOT_TOUCH", 1, "Don't touch this. This is for internal use only.");

            // config migrations
            if (CONFIG_VERSION.Value == 1) {
                CONFIG_VERSION.Value = 2;
                if (THREADPOOL_MAX_THREADS.Value == 32) {
                    THREADPOOL_MAX_THREADS.Value = 4;
                    SoundPlugin.logger.LogInfo("Migrated config info.");
                }
            }
        }
    }
}
