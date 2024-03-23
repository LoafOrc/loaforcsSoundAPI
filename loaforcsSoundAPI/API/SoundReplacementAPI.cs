using loaforcsSoundAPI.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public static class SoundReplacementAPI {
        internal static Dictionary<string, AudioFormatProvider> FileFormats = new Dictionary<string, AudioFormatProvider>();
        internal static Dictionary<string, RandomProvider> RandomProviders = new Dictionary<string, RandomProvider>();
        internal static Dictionary<string, ConditionProvider> ConditionProviders = new Dictionary<string, ConditionProvider>();
        internal static Dictionary<string, VariableProvider> VariableProviders = new Dictionary<string, VariableProvider>();

        internal static ConcurrentDictionary<string, List<SoundReplacementCollection>> SoundReplacements = new ConcurrentDictionary<string, List<SoundReplacementCollection>>();


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

        public static string FormatMatchString(string input) {
            if (input.Split(":").Length == 2) {
                input = "*:" + input;
            }
            return input;
        }

        public static bool MatchStrings(string a, string b) {
            SoundPlugin.logger.LogDebug($"{a} == {b}?");
            string[] testing = a.Split(":");
            string[] expected = b.Split(":");
            if (expected[0] != "*" && expected[0] != testing[0]) return false; // parent gameobject mismatch
            if (expected[1] != "*" && expected[1] != testing[1]) return false; // gameobject mismatch
            return testing[2] == expected[2];
        }
    }
}
