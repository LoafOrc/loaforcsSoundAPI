using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions {
    internal class ConfigCondition : ConditionProvider {
        public override bool Evaluate(SoundPack pack, JObject conditionDef) {
            return pack.GetConfigOption<bool>((string)conditionDef["config"]);
        }
    }
}
