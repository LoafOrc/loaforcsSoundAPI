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

        void Start() {
            if(source == null) {
                SoundPlugin.logger.LogWarning($"AudioSource (on gameobject: {gameObject.name}) became null between the OnSceneLoaded callback and Start. This is most likely because of another mod.");
                return;
            }

            if (source.playOnAwake && source.enabled) {
                source.Play();
            }

            helpers[source] = this;
        }

        void OnDestroy() {
            helpers.Remove(source);
        }

        void LateUpdate() {
            if (replacedWith == null) return;
            if (!replacedWith.group.UpdateEveryFrame) return;

            float currentTime = source.time;

            List<SoundReplacement> replacements = replacedWith.replacements.Where(x => x.TestCondition()).ToList();
            int totalWeight = 0;

            replacements.ForEach(replacement => totalWeight += replacement.Weight);

            int chosenWeight = replacedWith.group.Random.Range(replacedWith.group, 0, totalWeight);
            int chosen = 0;
            while (chosenWeight > 0) {
                chosen = replacedWith.group.Random.Range(replacedWith.group, 0, replacements.Count);
                chosenWeight -= replacedWith.group.Random.Range(replacedWith.group, 1, replacements[chosen].Weight);
            }

            source.clip = replacements[chosen].Clip;
            source.time = currentTime;
        }
    }
}
