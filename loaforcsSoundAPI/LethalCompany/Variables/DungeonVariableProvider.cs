using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Variables;
internal class DungeonVariableProvider : VariableProvider
{
    public override object Evaluate(SoundReplaceGroup group, JObject varDef)
    {
        return RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name;
    }
}
