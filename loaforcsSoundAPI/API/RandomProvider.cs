using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public abstract class RandomProvider {
        public abstract int Range(SoundReplaceGroup group, int min, int max);
    }
}
