using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Conditions;
internal class TimeOfDayConditionProvider : ConditionProvider {
    public override bool Evaluate(SoundReplaceGroup group, JObject varDef) {
        return varDef["time_of_day"].Value<string>() == TimeOfDay.Instance.dayMode.ToString().ToLower();
    }
}