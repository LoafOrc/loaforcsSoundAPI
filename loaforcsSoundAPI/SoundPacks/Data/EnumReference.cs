using System;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class EnumReference<T> : ContentReference<T> where T : struct, Enum {
    public EnumReference(string input) : base(input) => Resolve();

    /// <inheritdoc/>
    protected override bool TryResolve(string input, out T value) => Enum.TryParse(input, ignoreCase: true, out value);
}