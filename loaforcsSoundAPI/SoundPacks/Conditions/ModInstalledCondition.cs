using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

/// <summary>
/// Checks if a mod is installed. Uses a mod's GUID.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>mod_installed</id>
/// </soundapi>
[SoundAPICondition("mod_installed")]
class ModInstalledCondition : Condition {
	/// <summary>
	/// Mod GUID to check
	/// </summary>
	/// <value><see cref="string"/></value>
	/// <example>me.loaforc.facilitymeltdown</example>
	public string Value { get; private set; }

	public override bool CanBeImpliedConstant() {
		return true;
	}

	public override bool Evaluate(IContext context) {
		return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(Value);
	}

	public override List<IValidatable.ValidationResult> Validate() {
		if(string.IsNullOrEmpty(Value)) {
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Value on 'mod_installed' must be there and must not be empty.")
			];
		}

		return [ ];
	}
}