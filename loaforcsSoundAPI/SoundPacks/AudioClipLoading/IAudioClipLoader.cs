using System.Collections.Generic;
using loaforcsSoundAPI.SoundPacks.Data;
using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.AudioClipLoading;

// todo: could probably use a better name
interface IAudioClipLoader {
	internal static readonly Dictionary<string, AudioType> audioExtensions = new Dictionary<string, AudioType> {
		{ ".ogg", AudioType.OGGVORBIS },
		{ ".wav", AudioType.WAV },
		{ ".mp3", AudioType.MPEG }
	};

	void Queue(SoundInstance sound);
}