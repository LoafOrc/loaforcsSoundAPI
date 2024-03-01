using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Variables;
internal class ConfigVariableProvider : VariableProvider {
    public override object Evaluate(SoundReplaceGroup group, JObject varDef) {
        return group.pack.GetConfigOption<object>((string)varDef["config"]);
    }
}
