using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Conditions;
internal class PlayerHealthConditionProvider : ConditionProvider {
    public override bool Evaluate(SoundReplaceGroup group, JObject conditionDef) {
        return EvaluateRangeOperator(GameNetworkManager.Instance.localPlayerController.health, conditionDef["health"].Value<string>());
    }
}