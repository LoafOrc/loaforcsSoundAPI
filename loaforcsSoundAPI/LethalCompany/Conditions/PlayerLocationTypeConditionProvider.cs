using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Conditions;
internal class PlayerLocationTypeConditionProvider : ConditionProvider {
    public override bool Evaluate(SoundReplaceGroup group, JObject varDef) {
        if(GameNetworkManager.Instance.localPlayerController.isPlayerDead) return false;
        if(GameNetworkManager.Instance.localPlayerController.isInsideFactory) {
            return varDef["location"].Value<string>() == "inside";
        }
        if(StartOfRound.Instance.shipBounds.bounds.Contains(GameNetworkManager.Instance.localPlayerController.transform.position)) {
            return varDef["location"].Value<string>() == "on_ship";
        }
        return varDef["location"].Value<string>() == "outside";
    }
}