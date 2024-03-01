using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Variables;
internal class MoonVariableProvider : VariableProvider {
    public override object Evaluate(SoundReplaceGroup group, JObject varDef) {
        return StartOfRound.Instance.currentLevel.name;
    }
}
