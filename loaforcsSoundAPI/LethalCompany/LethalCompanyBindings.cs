using loaforcsSoundAPI.API;
using loaforcsSoundAPI.LethalCompany.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany;
internal static class LethalCompanyBindings {
    internal static void Bind() {
        SoundAPI.RegisterConditionProvider("LethalCompany:dungeon_name", new DungeonConditionProvider());
        SoundAPI.RegisterConditionProvider("LethalCompany:moon_name", new MoonConditionProvider());
        SoundAPI.RegisterConditionProvider("LethalCompany:player_health", new PlayerHealthConditionProvider());
        SoundAPI.RegisterConditionProvider("LethalCompany:time_of_day", new TimeOfDayConditionProvider());
        SoundAPI.RegisterConditionProvider("LethalCompany:player_location", new PlayerLocationTypeConditionProvider());
    }
}
