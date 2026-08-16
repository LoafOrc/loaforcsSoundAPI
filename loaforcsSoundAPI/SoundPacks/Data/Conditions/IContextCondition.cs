using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Data.Conditions;

/// <summary>
/// Condition with Context interface.
/// </summary>
/// <typeparam name="TContext">Context to use.</typeparam>
public interface IContextCondition<TContext> where TContext : IContext {
    /// <summary>
    /// Evaluate Condition. If the context type of the parameter does not match this condition, it will evaluate using the fallback.
    /// </summary>
    /// <param name="context">Any possible context.</param>
    /// <returns>If condition succeeds.</returns>
    bool Evaluate(IContext context);

    /// <summary>
    /// Context type matches.
    /// </summary>
    /// <param name="context">At least the correct context type, but could be any inherited class.</param>
    /// <returns>If condition succeeds.</returns>
    bool EvaluateWithContext(TContext context);

    /// <summary>
    /// Context type did not match.
    /// </summary>
    /// <param name="context">Unknown context type.</param>
    /// <returns>If condition succeeds.</returns>
    bool EvaluateFallback(IContext context);
}