using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Properties;
using UnityEngine;

namespace loaforcsSoundAPI.Behaviours {
    public class AudioSourceReplaceHelper : MonoBehaviour {
        internal AudioSource source;
        internal float originalVolume;
        internal SoundReplaceGroup replacedGroup;

        internal static Dictionary<AudioSource, AudioSourceReplaceHelper> helpers = new Dictionary<AudioSource, AudioSourceReplaceHelper>();

        void Start() {
            SoundPlugin.logger.LogDebug(gameObject.name);

            
            if (source.playOnAwake && source.enabled) {
                source.Play();
            }
            originalVolume = source.volume;
            
        }

        void LateUpdate() {
            // source.volume = originalVolume;
        }
    }
}
