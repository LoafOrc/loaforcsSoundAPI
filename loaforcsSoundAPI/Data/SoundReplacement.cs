using UnityEngine;

namespace loaforcsSoundAPI.Data {
    internal class SoundReplacement {
        public string SoundPath { get; set; }
        public int Weight = 1;

        public AudioClip Clip { get; set; }

        public SoundReplaceGroup group { get; set; }
    }
}
