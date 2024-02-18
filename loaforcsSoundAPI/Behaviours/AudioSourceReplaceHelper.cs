using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace loaforcsSoundAPI.Behaviours {
    public class AudioSourceReplaceHelper : MonoBehaviour {
        internal AudioSource source;

        [SerializeField]
        public bool playOnAwake;

        void Start() {
            SoundPlugin.logger.LogDebug(gameObject.name);

            if(playOnAwake && source.enabled) {
                source.Play();
            }
        }
    }
}
