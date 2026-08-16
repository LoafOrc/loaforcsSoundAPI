using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using Newtonsoft.Json;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

/// <summary>
/// Inverts the result of a condition.
/// `true` -&gt; `false`
/// `false` -&gt; `true`
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>not</id>
/// </soundapi>
[SoundAPICondition("not")]
class NotCondition : Condition {
	/// <summary>
	/// Condition to invert
	/// </summary>
	/// <value><see cref="Condition"/></value>
	public Condition Condition { get; private set; }

	public override void OnRegistered() {
		if(Condition != null) {
			Condition.Parent = Parent;
			Condition.OnRegistered();
		}
	}

	public override bool Evaluate(IContext context) {
		if(Condition is InvalidCondition) return false;
		return !Condition.Evaluate(context);
	}

	public override List<IValidatable.ValidationResult> Validate() {
		if(Condition == null) {
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, "'not' condition has no valid condition to invert!")
			];
		}

		return Condition.Validate();
	}
}