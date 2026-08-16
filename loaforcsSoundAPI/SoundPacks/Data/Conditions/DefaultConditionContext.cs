using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Data.Conditions;

class DefaultConditionContext(AudioSource source) : IContext {
	internal static readonly DefaultConditionContext DEFAULT = new DefaultConditionContext(null);

	public AudioSource Source => source;
}