using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public abstract class ConditionProvider {
        public abstract bool Evaluate(SoundReplaceGroup group, JObject conditionDef);

        public bool EvaluateRangeOperator(JToken number, string condition) {
            if(number.Type == JTokenType.Float) return EvaluateRangeOperator(number.Value<float>(), condition);
            return EvaluateRangeOperator(number.Value<int>(), condition);
        }

        public bool EvaluateRangeOperator(int number, string condition) {
            return EvaluateRangeOperator((double)number, condition);
        }

        public bool EvaluateRangeOperator(float number, string condition) {
            return EvaluateRangeOperator((double) number, condition);
        }

        // definently not created by chatgpt
        public bool EvaluateRangeOperator(double number, string condition) {
            // Splitting the condition string by ".."
            string[] parts = condition.Split("..");

            if(parts.Length == 1) {
                // Case when there's only one number in the condition
                double target;
                if(double.TryParse(parts[0], out target)) {
                    return (number == target);
                } else {
                    // Invalid input
                    return false;
                }
            } else if(parts.Length == 2) {
                // Case when there's a range specified
                double lowerBound, upperBound;

                if(parts[0] == "") {
                    lowerBound = double.MinValue;
                } else {
                    if(!double.TryParse(parts[0], out lowerBound)) {
                        // Invalid input
                        return false;
                    }
                }

                if(parts[1] == "") {
                    upperBound = double.MaxValue;
                } else {
                    if(!double.TryParse(parts[1], out upperBound)) {
                        // Invalid input
                        return false;
                    }
                }

                return (number >= lowerBound && number <= upperBound);
            } else {
                // Invalid input
                return false;
            }
        }
    }
}
