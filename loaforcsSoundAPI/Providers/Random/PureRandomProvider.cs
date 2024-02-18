using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Random {
    internal class PureRandomProvider : RandomProvider {
        public override int Range(SoundReplaceGroup group, int min, int max) {
            return UnityEngine.Random.Range(min, max);
        }
    }
}
