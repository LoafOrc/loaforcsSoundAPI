using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.Providers.Conditions;
internal class ModInstalledConditionProvider : ConditionProvider {
    public override bool Evaluate(SoundReplaceGroup group, JObject conditionDef) {
        return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey((string)conditionDef["mod_guid"]);
    }
}
