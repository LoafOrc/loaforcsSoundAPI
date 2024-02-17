using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace loaforcsSoundAPI.Behaviours {
    public class AudioSourceReplaceHelper : MonoBehaviour {
        internal AudioSource source;

        [SerializeField]
        public bool playOnAwake;

        void OnEnable() {
            if(playOnAwake) {
                source.Play();
            }
        }
    }
}
