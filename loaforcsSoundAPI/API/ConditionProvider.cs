using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public abstract class ConditionProvider {
        public abstract bool Evaluate(SoundReplaceGroup group, JObject conditionDef);

        protected bool TryEvaluateVariale(SoundReplaceGroup group, JToken variableDef, out object value) {
            value = null;
            if (variableDef is JObject) {
                try {
                    value = SoundReplacementAPI.VariableProviders[(string)(variableDef["type"])].Evaluate(group, variableDef as JObject);
                } catch (Exception e) {
                    SoundPlugin.logger.LogError(e);
                    return false;
                }
            } else {
                value = (variableDef as JValue).Value;
            }
            return true;
        }
    }
}
