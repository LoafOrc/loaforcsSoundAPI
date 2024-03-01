using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Variables;
internal class TimeOfDayVariableProvider : VariableProvider {
    public override object Evaluate(SoundReplaceGroup group, JObject varDef) {
        return TimeOfDay.Instance.dayMode.ToString().ToLower();
    }
}