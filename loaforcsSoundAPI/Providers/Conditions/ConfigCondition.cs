using BepInEx.Configuration;
using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using loaforcsSoundAPI.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions {
    internal class ConfigCondition : ConditionProvider {
        public override bool Evaluate(SoundReplaceGroup group, JObject conditionDef) {
            if(!conditionDef.ContainsKey("value")) {
                return group.pack.GetConfigOption<bool>((string)conditionDef["config"]);
            }
            if(conditionDef["value"].Type == JTokenType.Boolean) {
                return group.pack.GetConfigOption<bool>((string)conditionDef["config"]) == (bool) conditionDef["value"];
            }
            object config = group.pack.GetRawConfigOption((string)conditionDef["config"]);
            if(conditionDef["value"].Type == JTokenType.String) {
                if((config is ConfigEntry<float>)) {
                    return EvaluateRangeOperator((config as ConfigEntry<float>).Value, (string)conditionDef["value"]);
                } else {
                    return (string)config == (string)conditionDef["value"];
                }
            }

            return false;
        }
    }
}
