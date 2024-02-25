using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Variables;
internal class PlayerHealthVariableProvider : VariableProvider {
    public override object Evaluate(SoundReplaceGroup group, JObject varDef) {
        return GameNetworkManager.Instance.localPlayerController.health;
    }
}