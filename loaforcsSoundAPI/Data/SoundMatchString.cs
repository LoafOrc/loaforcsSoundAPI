using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Data {
    internal class SoundMatchString {
        private string Match; // eg MenuManager:Menu1
        public SoundReplaceGroup Group {  get; private set; }

        public string ParentName { get { return Match.Split(":")[0]; } }
        public string AudioName { get { return Match.Split(":")[2]; } }
        public string GameObjectName { get { return Match.Split(":")[1]; } }

        public bool Matches(string match) {
            string[] testing = match.Split(":");
            string[] expected = Match.Split(":");
            if (expected[0] != "*" && expected[0] != testing[0]) return false; // parent gameobject mismatch
            if (expected[1] != "*" && expected[1] != testing[1]) return false; // gameobject mismatch
            return testing[2] == expected[2];
        }

        public SoundMatchString(SoundReplaceGroup group, string Match) {
            if(Match.Split(":").Length == 2) {
                this.Match = "*:" + Match;
            } else {
                this.Match = Match;
            }
            this.Group = group;
        }
    }
}
