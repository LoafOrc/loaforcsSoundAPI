using loaforcsSoundAPI.Patches;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.VFX.VisualEffectControlTrackController;

namespace loaforcsSoundAPI.Behaviours {
    internal class SoundReplacerHandler : MonoBehaviour {
        void Start() {
            SceneManager.sceneLoaded += ProcessNewScene;
        }
        void OnDestroy() {
            SceneManager.sceneLoaded += ProcessNewScene;
        }

        void ProcessNewScene(Scene scene, LoadSceneMode __) {
            foreach(AudioSource source in FindObjectsOfType<AudioSource>(true)) {
                if(source.gameObject.scene != scene) continue; // already processed

                if(source.playOnAwake)
                   source.Stop();

                AudioSourceReplaceHelper ext = source.gameObject.AddComponent<AudioSourceReplaceHelper>();
                ext.source = source;
            }
        }
    }
}
