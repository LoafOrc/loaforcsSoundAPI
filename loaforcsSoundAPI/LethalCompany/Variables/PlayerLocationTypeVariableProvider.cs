using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Variables;
internal class PlayerLocationTypeVariableProvider : VariableProvider {
    public override object Evaluate(SoundReplaceGroup group, JObject varDef) {
        if(GameNetworkManager.Instance.localPlayerController.isPlayerDead) return null;
        if(GameNetworkManager.Instance.localPlayerController.isInsideFactory) {
            return "inside";
        }
        if(StartOfRound.Instance.shipBounds.bounds.Contains(GameNetworkManager.Instance.localPlayerController.transform.position)) {
            return "on_ship";
        }
        return "outside";
    }
}