using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Data.Conditions;

/// <summary>
/// Context interface.
/// </summary>
public interface IContext {
    public AudioSource Source { get; }
}