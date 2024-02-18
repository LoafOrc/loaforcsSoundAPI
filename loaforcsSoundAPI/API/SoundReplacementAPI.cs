using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public static class SoundReplacementAPI {
        internal static Dictionary<string, AudioFormatProvider> FileFormats = new Dictionary<string, AudioFormatProvider>();
        internal static Dictionary<string, RandomProvider> RandomProviders = new Dictionary<string, RandomProvider>();

        public static void RegisterAudioFormatProvider(string extension, AudioFormatProvider provider) {
            FileFormats.Add(extension, provider);
        }

        public static void RegisterRandomProvider(string extension, RandomProvider provider) {
            RandomProviders.Add(extension, provider);
        }
    }
}
