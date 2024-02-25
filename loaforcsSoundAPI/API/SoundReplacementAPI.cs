using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public static class SoundReplacementAPI {
        internal static Dictionary<string, AudioFormatProvider> FileFormats = new Dictionary<string, AudioFormatProvider>();
        internal static Dictionary<string, RandomProvider> RandomProviders = new Dictionary<string, RandomProvider>();
        internal static Dictionary<string, ConditionProvider> ConditionProviders = new Dictionary<string, ConditionProvider>();
        internal static Dictionary<string, VariableProvider> VariableProviders = new Dictionary<string, VariableProvider>();

        public static void RegisterAudioFormatProvider(string extension, AudioFormatProvider provider) {
            FileFormats.Add(extension, provider);
        }

        public static void RegisterRandomProvider(string extension, RandomProvider provider) {
            RandomProviders.Add(extension, provider);
        }
        public static void RegisterConditionProvider(string extension, ConditionProvider provider) {
            ConditionProviders.Add(extension, provider);
        }
        public static void RegisterVariableProvider(string extension, VariableProvider provider) {
            VariableProviders.Add(extension, provider);
        }
    }
}
