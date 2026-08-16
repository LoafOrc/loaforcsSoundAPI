namespace loaforcsSoundAPI.SoundPacks.Data;

public abstract class ContentReference {
    public abstract int Uses { get; internal set; }
    public abstract bool Resolved { get; }
    public abstract void Resolve();
}

public abstract class ContentReference<T>(string input) : ContentReference {
    public T Value { get => _value; }
    T _value;

    public sealed override int Uses { get; internal set; }

    public sealed override bool Resolved { get => _resolved && _value != null; }
    bool _resolved;

    public sealed override void Resolve() {
        if(_resolved) return;
        if(string.IsNullOrEmpty(input)) return;

        _resolved = TryResolve(input, out _value);
        OnResolved(_resolved);
    }
    protected virtual void OnResolved(bool success) { }

    protected abstract bool TryResolve(string input, out T value);

    public static implicit operator T(ContentReference<T> reference) => reference._value;

    /// <inheritdoc/>
    public override string ToString() {
        return $"\"{input}\" | Resolved: {Resolved} | Uses: {Uses}";
    }
}