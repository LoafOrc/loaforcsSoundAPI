using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Properties;
using UnityEngine;

namespace loaforcsSoundAPI.Behaviours {
    public class AudioSourceReplaceHelper : MonoBehaviour {
        internal AudioSource source;
        internal SoundReplaceGroup replacedGroup;

        internal static Dictionary<AudioSource, AudioSourceReplaceHelper> helpers = new Dictionary<AudioSource, AudioSourceReplaceHelper>();

        void Start() {
            if(gameObject == null) { // will prob never happen but ac could be doing some silly stuff
                // OH GOD, OH NO, WHAT HAPPENED
                SoundPlugin.logger.LogError("AAAAAAAAAAA OH GOD, OH NO, WHY IS THE GAMEOBJECT NULL??? THIS IS NOT GOOD. NOT GOOD AT ALL");
                return;
            }
            SoundPlugin.logger.LogDebug(gameObject.name);

            if(source == null) {
                SoundPlugin.logger.LogWarning($"AudioSource (gameObject: {gameObject.name}) become null sometime between OnSceneLoaded() and .Start(), Most likely AdvancedCompany doing some things...");
                return;
            }

            if (source.playOnAwake && source.enabled) {
                source.Play();
            }
            
        }

        void LateUpdate() {
            // source.volume = originalVolume;
        }
    }
}
