using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Random {
    internal class DeterminsticRandomProvider : RandomProvider {
        internal static Dictionary<SoundReplaceGroup, System.Random> Generators = new Dictionary<SoundReplaceGroup, System.Random>();

        public override int Range(SoundReplaceGroup group, int min, int max) {
            if(!Generators.TryGetValue(group, out var result)) {
                result = new System.Random(group.pack.Name.GetHashCode());
                Generators[group] = result;
            }

            return result.Next(min, max);
        }
    }
}
