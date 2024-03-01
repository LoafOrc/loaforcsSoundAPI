using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions;
internal class NotCondition : ConditionProvider {
    public override bool Evaluate(SoundReplaceGroup pack, JObject conditionDef) {
        return !SoundReplacementAPI.ConditionProviders[(string)conditionDef["condition"]["type"]].Evaluate(pack, conditionDef["condition"] as JObject);
    }
}