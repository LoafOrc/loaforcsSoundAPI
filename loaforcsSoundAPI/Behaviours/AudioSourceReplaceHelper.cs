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
            if(gameObject == null) {
                // OH GOD, OH NO, WHAT HAPPENED
                SoundPlugin.logger.LogError("AAAAAAAAAAA OH GOD, OH NO, WHY IS THE GAMEOBJECT NULL??? THIS IS NOT GOOD. NOT GOOD AT ALL");
                return;
            }
            SoundPlugin.logger.LogDebug(gameObject.name);

            if(source == null) {
                SoundPlugin.logger.LogWarning($"AudioSource (on gameobject: {gameObject.name}) became null between LevelLoad callback and Start. This shouldn't have happened. Most likely another mod caused this.");
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
