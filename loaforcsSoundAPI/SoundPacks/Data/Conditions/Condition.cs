using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.Core.Data;

namespace loaforcsSoundAPI.SoundPacks.Data.Conditions;

/// <summary>
/// Non-generic Condition.
/// </summary>
/// <seealso cref="Condition{ContextType}"/>
/// <seealso cref="IContext"/>
public abstract class Condition : IValidatable, IRegistrationCallback {
	[field: NonSerialized]
	public Conditional Parent { get; internal set; }

	/// <summary>
	/// Utility property to quickly access an instance of a condition's <see cref="SoundPack"/>
	/// </summary>
	protected SoundPack Pack => Parent.Pack;

	/// <summary>
	/// When a condition is explicitly set to 'constant' it will compute the value on load.
	/// The 
	/// todo: For the config condition the Constant value should be implied to be true
	/// </summary>
	public bool? Constant { get; private set; }

	/// <summary>
	/// Determines if this condition can be implied constant.
	/// A constant condition allows sounds to skip loading, saving memory and startup time
	/// </summary>
	/// <returns></returns>
	public virtual bool CanBeImpliedConstant() {
		return false;
	}

	public virtual void OnRegistered() { }

	/// <summary>
	/// Evaluate Condition
	/// </summary>
	/// <param name="context">Any possible context</param>
	/// <returns>If condition succeeds</returns>
	public abstract bool Evaluate(IContext context);

	/// <inheritdoc />
	public virtual List<IValidatable.ValidationResult> Validate() {
		return [ ];
	}

	protected static void LogDebug(string name, object message) {
		Debuggers.ConditionsInfo?.Log($"({name}) {message}");
	}

	public static bool ShouldBeMadeConstant(Condition condition) {
		if(condition.Constant == true) return true;
		return condition.CanBeImpliedConstant() && PackLoadingConfig.SkipUnusedSounds;
	}
}

sealed class InvalidCondition(string type) : Condition {
	public override bool Evaluate(IContext context) {
		return false;
	}

	public override List<IValidatable.ValidationResult> Validate() {
		if(string.IsNullOrEmpty(type)) {
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, "Condition must have a type!")
			];
		} else {
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"'{type}' is not a valid condition type!")
			];
		}
	}
}

sealed class ConstantCondition : Condition {
	public static ConstantCondition TRUE = new ConstantCondition(true);
	public static ConstantCondition FALSE = new ConstantCondition(false);

	public bool Value { get; private set; }

	ConstantCondition(bool constant) {
		Value = constant;
	}

	public override bool Evaluate(IContext context) {
		return Value;
	}
}

/// <summary>
/// A generic version of Condition to simplify working with Contexts.
/// </summary>
/// <seealso cref="Condition"/>
/// <seealso cref="IContext"/>
/// <typeparam name="TContext">Type of context</typeparam>
public abstract class Condition<TContext> : Condition, IContextCondition<TContext> where TContext : struct, IContext {
	/// <summary>
	/// Evaluate Condition. If the context type of the parameter does not match this condition, it will evaluate using the fallback.
	/// </summary>
	/// <param name="context">Any possible context</param>
	/// <returns>If condition succeeds</returns>
	public override bool Evaluate(IContext context) {
		if(context is not TContext type) return EvaluateFallback(context); // mismatching context, use fallback

		return EvaluateWithContext(type);
	}

	/// <summary>
	/// Context type matches
	/// </summary>
	/// <param name="context">At least the correct context type, but could be any inherited class</param>
	/// <returns>If condition succeeds</returns>
	public abstract bool EvaluateWithContext(TContext context);

	/// <summary>
	/// Context type did not match
	/// </summary>
	/// <param name="context">Unknown context type</param>
	/// <returns>If condition succeeds</returns>
	public virtual bool EvaluateFallback(IContext context) {
		return false;
	}
}