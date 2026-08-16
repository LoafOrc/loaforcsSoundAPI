using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

public abstract class AnimatorCondition : Condition {
    /// <summary>
    /// Name of the <c>Animator</c> parameter to evaluate.
    /// </summary>
    public AnimatorHashReference Parameter { get; private set; }

    /// <summary>
    /// Type of the <c>Animator</c> parameter to evaluate.
    /// </summary>
    public EnumReference<AnimatorParamType> ParameterType { get; private set; }

    /// <summary>
    /// Value of the <c>Animator</c> parameter to evaluate, of any (valid) type.
    /// </summary>
    public string Value { get; private set; }
    [NonSerialized] protected bool _value;

    /// <summary>
    /// Floating value range operator, used if parameter type is <c>AnimatorParamType.Float</c>.
    /// </summary>
    public RangeOperator<float> FloatRange { get => _floatRange; }
    [NonSerialized] RangeOperator<float> _floatRange;

    /// <summary>
    /// Integer value range operator, used if parameter type is <c>AnimatorParamType.Integer</c>.
    /// </summary>
    public RangeOperator<int> IntRange { get => _intRange; }
    [NonSerialized] RangeOperator<int> _intRange;

    protected abstract string ValidateWarnMessage { get; }

    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        if(!TryGetAnimator(out Animator animator, context)) return false;

        switch(ParameterType.Value) {
            case AnimatorParamType.Bool or AnimatorParamType.Trigger:
                if(animator.GetBool(Parameter) == _value) return true;
                break;
            case AnimatorParamType.Float:
                if(FloatRange.EvaluateRange(animator.GetFloat(Parameter))) return true;
                break;
            case AnimatorParamType.Integer:
                if(IntRange.EvaluateRange(animator.GetInteger(Parameter))) return true;
                break;
            case AnimatorParamType.None:
            default:
                break;
        }

        return false;
    }

    /// <inheritdoc/>
    public override List<IValidatable.ValidationResult> Validate() {
        List<IValidatable.ValidationResult> results = [
            new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, ValidateWarnMessage)
        ];

        switch(ParameterType.Value) {
            case AnimatorParamType.Bool or AnimatorParamType.Trigger:
                if(!bool.TryParse(Value, out _value)) return results;
                break;
            case AnimatorParamType.Float:
                _floatRange = new RangeOperator<float>(Value, new(float.NegativeInfinity, float.PositiveInfinity));
                results = FloatRange.Validate();
                if(results.Count > 1) return results;
                break;
            case AnimatorParamType.Integer:
                _intRange = new RangeOperator<int>(Value, new(int.MinValue, int.MaxValue));
                results = IntRange.Validate();
                if(results.Count > 1) return results;
                break;
            case AnimatorParamType.None:
            default:
                break;
        }

        return base.Validate();
    }

    /// <summary>
    ///     Attempt to obtain the specific <c>Animator</c> instance this <c>Condition</c> should use to evaluate.
    /// </summary>
    /// <param name="animator"><c>Animator</c> instance to evaluate with.</param>
	/// <param name="context">Any possible context.</param>
    /// <returns>Whether an <c>Animator</c> was successfully obtained or not.</returns>
    protected abstract bool TryGetAnimator(out Animator animator, IContext context);
}

public abstract class AnimatorCondition<TContext> : AnimatorCondition, IContextCondition<TContext> where TContext : struct, IContext {
    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        if(context is not TContext type) return EvaluateFallback(context); // mismatching context, use fallback

        return EvaluateWithContext(type);
    }

    /// <inheritdoc/>
    public virtual bool EvaluateWithContext(TContext context) {
        return base.Evaluate(context);
    }

    /// <inheritdoc/>
    public virtual bool EvaluateFallback(IContext context) {
        return false;
    }

    /// <inheritdoc/>
    protected override bool TryGetAnimator(out Animator animator, IContext context) {
        animator = null;
        return (context is TContext contextWithType) && TryGetAnimator(out animator, contextWithType);
    }

    /// <summary>
    ///     Attempt to obtain the specific <c>Animator</c> instance this <c>Condition</c> should use to evaluate.
    /// </summary>
    /// <param name="animator"><c>Animator</c> instance to evaluate with.</param>
    /// <param name="context">Context of type <typeparamref name="TContext"/> to check.</param>
    /// <returns>Whether an <c>Animator</c> was successfully obtained or not.</returns>
    protected abstract bool TryGetAnimator(out Animator animator, TContext context);
}

public enum AnimatorParamType : byte {
    None,
    Bool,
    Float,
    Integer,
    Trigger
}