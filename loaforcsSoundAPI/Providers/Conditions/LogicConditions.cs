using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using loaforcsSoundAPI.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions;
internal class EqualsCondition : ConditionProvider {

    public override bool Evaluate(SoundReplaceGroup pack, JObject conditionDef) {
        if (!TryEvaluateVariale(pack, conditionDef["a"], out object a)) return false;
        if (!TryEvaluateVariale(pack, conditionDef["b"], out object b)) return false;

        if (a.IsNumber() && b.IsNumber()) {
            return (double)a == (double)b;
        }
        if (a is string && b is string) {
            bool case_sensitive = false;
            if (conditionDef.TryGetValue("case_sensitive", out JToken sensitive)) case_sensitive = (bool)sensitive;
            if(case_sensitive)
                return (string)a == (string)b;
            else
                return ((string)a).ToLower() == ((string)b).ToLower();
        }
        if(a is bool && b is bool) {
            return (bool)a == (bool)b;
        }

        SoundPlugin.logger.LogError($"Mismatching or Unknown Types! a: {a.GetType()} - b: {b.GetType()}");
        return false;
    }
}

internal class GreaterThanCondition : ConditionProvider {

    public override bool Evaluate(SoundReplaceGroup pack, JObject conditionDef) {
        if (!TryEvaluateVariale(pack, conditionDef["a"], out object a)) return false;
        if (!TryEvaluateVariale(pack, conditionDef["b"], out object b)) return false;

        if (a.IsNumber() && b.IsNumber()) {
            return (double)a > (double)b;
        }

        SoundPlugin.logger.LogError($"Mismatching or Unknown Types! a: {a.GetType()} - b: {b.GetType()}");
        return false;
    }
}

internal class GreaterThanOrEqualCondition : ConditionProvider {

    public override bool Evaluate(SoundReplaceGroup pack, JObject conditionDef) {
        if (!TryEvaluateVariale(pack, conditionDef["a"], out object a)) return false;
        if (!TryEvaluateVariale(pack, conditionDef["b"], out object b)) return false;

        if (a.IsNumber() && b.IsNumber()) {
            return (double)a >= (double)b;
        }

        SoundPlugin.logger.LogError($"Mismatching or Unknown Types! a: {a.GetType()} - b: {b.GetType()}");
        return false;
    }
}