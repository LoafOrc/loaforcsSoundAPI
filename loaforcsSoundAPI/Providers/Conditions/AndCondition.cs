using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions;
internal class AndCondition : ConditionProvider {
    public override bool Evaluate(SoundReplaceGroup pack, JObject conditionDef) {
        foreach(JObject condition in conditionDef["conditions"]) {
            if (!SoundReplacementAPI.ConditionProviders[(string)condition["type"]].Evaluate(pack, condition)) {
                return false; // a signle thing failed, we don't need to caluclate the rest
            }
        }

        return true;
    }
}