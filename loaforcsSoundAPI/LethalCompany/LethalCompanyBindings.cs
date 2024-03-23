using loaforcsSoundAPI.API;
using loaforcsSoundAPI.LethalCompany.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany;
internal static class LethalCompanyBindings {
    internal static void Bind() {
        SoundReplacementAPI.RegisterConditionProvider("LethalCompany:dungeon_name", new DungeonConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("LethalCompany:moon_name", new MoonConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("LethalCompany:player_health", new PlayerHealthConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("LethalCompany:time_of_day", new TimeOfDayConditionProvider());
        SoundReplacementAPI.RegisterConditionProvider("LethalCompany:player_location", new PlayerLocationTypeConditionProvider());
    }
}
