using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace loaforcsSoundAPI.API {
    public abstract class AudioFormatProvider {

        public abstract AudioClip LoadAudioClip(string path);

        protected AudioClip LoadFromUWR(string path, AudioType type) {
            AudioClip clip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, type)) {
                uwr.SendWebRequest();

                // we have to wrap tasks in try/catch, otherwise it will just fail silently
                try {
                    while (!uwr.isDone) {

                    }

                    if (uwr.result != UnityWebRequest.Result.Success) {
                        SoundPlugin.logger.LogError($"============");
                        SoundPlugin.logger.LogError($"UnityWebRequest failed while trying to get {path}. Full error below");
                        SoundPlugin.logger.LogError(uwr.error);
                        SoundPlugin.logger.LogError($"============");
                    } else {
                        clip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                } catch (Exception err) {
                    SoundPlugin.logger.LogError($"{err.Message}, {err.StackTrace}");
                }
            }

            return clip;
        }
    }
}
