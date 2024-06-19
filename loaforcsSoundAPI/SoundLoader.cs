using BepInEx;
using loaforcsSoundAPI.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace loaforcsSoundAPI {
    public static class SoundLoader {

        // example input
        // GetAudioClip("SoundAPI_testpack", "player/jump", "player_jump01");
        public static bool GetAudioClip(string packName, string folder, string soundName, out AudioClip clip) {
            SoundPlugin.logger.LogLosingIt($"Loading sound: {packName}/{folder}/{soundName}");
            // Check if file exists
            // get audio from disk
            // final checks
            //      result is null
            //      name empty check

            clip = null;
            if(!GetAudioPath(packName, folder, soundName, out string path)) {
                SoundPlugin.logger.LogError($"Failed getting path, check above for more detailed error!");
                return false;
            }

            // get clip
            clip = SoundAPI.FileFormats[Path.GetExtension(path).ToLower()].LoadAudioClip(path);

            // final checks
            if(clip == null) {
                SoundPlugin.logger.LogError($"Failed getting clip from disk, check above for more detailed error!");
                return false;
            }
            if (string.IsNullOrEmpty(clip.GetName()) ) {
                clip.name = soundName;
            }

            return true;
        }

        private static bool GetAudioPath(string packName, string folder, string soundName, out string path) {
            string fullPathFolder = Path.Combine(Paths.PluginPath, packName, "sounds", folder);
            path = Path.Combine(fullPathFolder, soundName);

            if (!Directory.Exists(fullPathFolder)) {
                SoundPlugin.logger.LogError($"{fullPathFolder} doesn't exist! (failed loading: {soundName})");
                path = null;
                return false;
            }

            if (!File.Exists(path)) {
                SoundPlugin.logger.LogError($"{path} is a valid folder but the sound contained doesn't exist!");
                path = null;
                return false;
            }

            return true;
        }
    }
}
