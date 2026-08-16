using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using Newtonsoft.Json;

namespace loaforcsSoundAPI.SoundPacks.Data;

public abstract class RangeOperator(RangeOperator bounds = null) : IValidatable {
    public abstract int Uses { get; internal set; }

    /// <inheritdoc/>
    public virtual List<IValidatable.ValidationResult> Validate() => Validate(bounds);

    /// <summary>
    /// Run validations with defined bounds.
    /// </summary>
    /// <param name="bounds">Limits for minimum and maximum values.</param>
    /// <returns>Non-successful validations.</returns>
    protected abstract List<IValidatable.ValidationResult> Validate(RangeOperator bounds);
}

public class RangeOperator<T> : RangeOperator where T : struct, IComparable<T>, IConvertible {
    public string Input { get; }

    public T Min { get => _min; }
    T _min;

    public T Max { get => _max; }
    T _max;

    public sealed override int Uses { get; internal set; }

    /// <summary>
    /// Create a <c>RangeOperator</c> from a given string. Used by <c>RangeOperatorConverter</c>.
    /// </summary>
    /// <remarks>Requires <c>IValidatable.Validate()</c> to be completed before evaluating any range values.</remarks>
    /// <param name="input">Value to parse.</param>
    /// <param name="bounds">Limits for minimum and maximum values.</param>
    [JsonConstructor]
    public RangeOperator(string input, RangeOperator<T> bounds = null) : base(bounds) {
        Input = input;
    }

    /// <summary>
    /// Create a <c>RangeOperator</c> with already-defined minimum and maximum values.
    /// </summary>
    /// <param name="min">Minimum value of type <c><typeparamref name="T"/></c>.</param>
    /// <param name="max">Maximum value of type <c><typeparamref name="T"/></c>.</param>
    /// <param name="bounds">Limits for minimum and maximum values.</param>
    public RangeOperator(T min, T max, RangeOperator<T> bounds = null) : base(bounds) {
        _min = min;
        _max = (min.CompareTo(max) < 0) ? max : min;
        Input = $"{this}";
    }

    /// <summary>
    /// Evaluate if a value of type <c><typeparamref name="T"/></c> is within the range.
    /// </summary>
    /// <param name="value">Value of type <c><typeparamref name="T"/></c>.</param>
    /// <returns>Whether the given value is within the range or not.</returns>
    public bool EvaluateRange(T value) => Min.CompareTo(value) <= 0 && Max.CompareTo(value) >= 0;

    /// <inheritdoc/>
    protected override List<IValidatable.ValidationResult> Validate(RangeOperator bounds) {
        if(string.IsNullOrEmpty(Input)) {
            return [
                new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Range operator can not be missing or empty!")
            ];
        }

        if(bounds is not RangeOperator<T> boundsWithType) {
            return [
                new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Range operator '{Input}' needs defined bounds in order to be validated!")
            ];
        }

        string[] parts = Input.Split("..", StringSplitOptions.None);
        switch(parts.Length) {
            case 1:
                // Case when there's only one number in the condition.
                T target = boundsWithType.Min;
                if(!TryConvert(parts[0], ref target, boundsWithType, out IValidatable.ValidationResult result)) {
                    return [result];
                }
                _min = target;
                _max = target;
                break;
            case 2:
                // Case when there's a range specified.
                T min = boundsWithType.Min;
                T max = boundsWithType.Max;
                if(!TryConvert(parts[0], ref min, boundsWithType, out result) || !TryConvert(parts[1], ref max, boundsWithType, out result)) {
                    return [result];
                }
                _min = min;
                _max = (min.CompareTo(max) < 0) ? max : min;
                break;
            case > 2:
                return [
                    new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Range operator '{Input}' uses '..' more than once!")
                ];
            default:
                break;
        }

        return [];
    }

    static bool TryConvert(string parameter, ref T value, RangeOperator<T> bounds, out IValidatable.ValidationResult result) {
        result = null;

        if(string.IsNullOrEmpty(parameter)) {
            return true;
        }

        try {
            value = (T) Convert.ChangeType(parameter, typeof(T));
        } catch(Exception e) {
            result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Failed to parse '{parameter}' as type '{typeof(T).FullName}': {e}");
            return false;
        }

        if(!bounds.EvaluateRange(value)) {
            result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Parameter '{parameter}' is outside allowed bounds: '{bounds}'");
            return false;
        }

        return true;
    }

    public override string ToString() {
        return $"{Min}..{Max}";
    }
}