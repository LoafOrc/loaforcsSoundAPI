using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                if(!source.playOnAwake) continue;
                source.Stop();

                if(source.TryGetComponent(out AudioSourceReplaceHelper ext)) {
                    SoundPlugin.logger.LogWarning("Multiple audio sources on one gameobject, this is unsupported right now!");
                } else {
                    ext = source.gameObject.AddComponent<AudioSourceReplaceHelper>();
                    ext.source = source;
                    ext.playOnAwake = true;
                    source.playOnAwake = false;
                }
            }
        }
    }
}
