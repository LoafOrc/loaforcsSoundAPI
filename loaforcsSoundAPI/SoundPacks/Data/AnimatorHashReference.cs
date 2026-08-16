using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class AnimatorHashReference : ContentReference<int> {
    public AnimatorHashReference(string input) : base(input) => Resolve();

    /// <inheritdoc/>
    protected override bool TryResolve(string input, out int value) {
        value = Animator.StringToHash(input);
        return true;
    }
}