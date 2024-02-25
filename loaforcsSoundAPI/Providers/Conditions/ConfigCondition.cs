using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions {
    internal class ConfigCondition : ConditionProvider {
        public override bool Evaluate(SoundReplaceGroup group, JObject conditionDef) {
            return group.pack.GetConfigOption<bool>((string)conditionDef["config"]);
        }
    }
}
