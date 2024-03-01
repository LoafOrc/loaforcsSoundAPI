using loaforcsSoundAPI.API;
using loaforcsSoundAPI.LethalCompany.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.LethalCompany;
internal static class LethalCompanyBindings {
    internal static void Bind() {
        SoundReplacementAPI.RegisterVariableProvider("CurrentDungeonName", new DungeonVariableProvider());
        SoundReplacementAPI.RegisterVariableProvider("CurrentMoonName", new MoonVariableProvider());
        SoundReplacementAPI.RegisterVariableProvider("PlayerHealth", new PlayerHealthVariableProvider());
        SoundReplacementAPI.RegisterVariableProvider("TimeOfDayType", new TimeOfDayVariableProvider());
        SoundReplacementAPI.RegisterVariableProvider("PlayerLocationType", new PlayerLocationTypeVariableProvider());
    }
}
