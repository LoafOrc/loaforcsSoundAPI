﻿using loaforcsSoundAPI.API;
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
        return conditionDef["name"].Value<string>() == RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name;
    }
}
