using System;

namespace loaforcsSoundAPI;

/// <summary>
/// <see cref="SoundAPI"/>
/// </summary>
[Flags]
public enum AudioSourceCopyFlags {
	DontCopyPlayOnAwake = 1 << 0,
	DontCopySpatialize = 1 << 1,
	DontCopyLoop = 1 << 2
}