using loaforcsSoundAPI.API;
using loaforcsSoundAPI.LethalCompany.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany;
internal static class LethalCompanyBindings {
    internal static void Bind() {
        SoundReplacementAPI.RegisterConditionProvider("CurrentDungeonName", new DungeonConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("CurrentMoonName", new MoonConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("PlayerHealth", new PlayerHealthConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("TimeOfDayType", new TimeOfDayConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("PlayerLocationType", new PlayerLocationTypeConditionProvider());
    }
}
