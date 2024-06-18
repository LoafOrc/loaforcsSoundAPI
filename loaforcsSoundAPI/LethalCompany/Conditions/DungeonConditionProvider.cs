using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany.Conditions;
internal class DungeonConditionProvider : ConditionProvider
{
    public override bool Evaluate(SoundReplaceGroup group, JObject conditionDef)
    {
        SoundPlugin.logger.LogExtended("LethalCompany:dungeon_name value: " + RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name);
        return conditionDef["value"].Value<string>() == RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name;
    }
}
