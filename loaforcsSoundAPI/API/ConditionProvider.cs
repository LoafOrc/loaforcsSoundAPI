using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public abstract class ConditionProvider {
        public abstract bool Evaluate(SoundPack pack, JObject conditionDef);
    }
}
