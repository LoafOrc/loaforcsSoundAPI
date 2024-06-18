using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions;
internal class OrCondition : ConditionProvider {
    public override bool Evaluate(SoundReplaceGroup pack, JObject conditionDef) {
        foreach(JObject condition in conditionDef["conditions"]) {
            if (SoundAPI.ConditionProviders[(string)condition["type"]].Evaluate(pack, condition)) {
                return true; // a signle thing succeded, we don't need to caluclate the rest
            }
        }

        return false;
    }
}