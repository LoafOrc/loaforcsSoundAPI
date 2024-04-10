using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Properties;
using UnityEngine;

namespace loaforcsSoundAPI.Behaviours {
    public class AudioSourceReplaceHelper : MonoBehaviour {
        internal AudioSource source;
        internal SoundReplacementCollection replacedWith;

        internal static Dictionary<AudioSource, AudioSourceReplaceHelper> helpers = new Dictionary<AudioSource, AudioSourceReplaceHelper>();

        public bool DisableReplacing { get; private set; } = false;

        void Start() {
            if(source == null) {
                SoundPlugin.logger.LogWarning($"AudioSource (on gameobject: {gameObject.name}) became null between the OnSceneLoaded callback and Start.");
                return;
            }

            if(source.playOnAwake && source.enabled) {
                source.Play();
            }

            helpers[source] = this;
        }

        void OnEnable() {
            if(source == null) return;

            helpers[source] = this;
        }

        void OnDestroy() {
            if(source == null) return;
            if(helpers.ContainsKey(source))          
                helpers.Remove(source);
        }

        void LateUpdate() {
            if (replacedWith == null) return;
            if (!replacedWith.group.UpdateEveryFrame) return;
            DisableReplacing = true;

            float currentTime = source.time;

            SoundReplacement replacement = replacedWith.replacements.Where(x => x.TestCondition()).ToList()[0];

            if(replacement.Clip == source.clip) {
                return;
            }

            source.clip = replacement.Clip;
            source.Play();
            source.time = currentTime;
        }
    }
}
