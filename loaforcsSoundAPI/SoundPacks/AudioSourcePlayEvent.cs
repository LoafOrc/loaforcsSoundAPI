using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks;

public readonly struct AudioSourcePlayEvent {
	public AudioSourcePlayEvent(AudioSource source, AudioClip clip, bool isOneShot) {
		IsOneShot = isOneShot;
		Source = source;
		Clip = clip;
		Data = AudioSourceAdditionalData.GetOrCreate(source);
		Context = Data.CurrentContext ?? new DefaultConditionContext(source); // todo: support context overrides maybe?
	}

	public bool IsOneShot { get; }
	public AudioSource Source { get; }
	public AudioSourceAdditionalData Data { get; }
	public IContext Context { get; }
	public AudioClip Clip { get; }
}