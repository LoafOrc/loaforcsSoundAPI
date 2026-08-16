using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;

namespace loaforcsSoundAPI.Core;

/// <summary>
/// Contains additional data for a specific audio source.
/// </summary>
public class AudioSourceAdditionalData {
	internal AudioSourceAdditionalData(AudioSource source) {
		Source = source;
	}

	/// <summary>
	/// AudioSource that this AdditonalData is describing.
	/// </summary>
	public AudioSource Source { get; private set; }

	SoundReplacementGroup _replacedWith;

	/// <summary>
	/// AudioClip before replacement, this may differ from <see cref="RealClip"/>.
	/// </summary>
	public AudioClip OriginalClip { get; internal set; }

	/// <summary>
	/// AudioClip that will actually be played by Unity
	/// </summary>
	/// <remarks>
	/// This should be used almost everywhere internally in SoundAPI when updating the AudioClip on an AudioSource.
	/// </remarks>
	public AudioClip RealClip {
		get {
			using(new SpoofBypassContext()) {
				if(Debuggers.AudioSourceAdditionalData != null) {
					string realClip = "null";
					if(Source.clip) {
						realClip = Source.clip.name;
					}

					string originalClip = "null";
					if(OriginalClip) {
						originalClip = OriginalClip.name;
					}

					Debuggers.AudioSourceAdditionalData.Log($"({Source.name}) Getting real clip: {realClip} (original clip: {originalClip})");
				}

				return Source.clip;
			}
		}
		set {
			using(new SpoofBypassContext()) {
				if(Debuggers.AudioSourceAdditionalData != null) {
					string originalClip = "null";
					if(OriginalClip) {
						originalClip = OriginalClip.name;
					}

					Debuggers.AudioSourceAdditionalData?.Log($"({Source.name}) Setting real clip: {value.name} (original clip: {originalClip})");
				}

				Source.clip = value;
			}
		}
	}

	public SoundReplacementGroup ReplacedWith {
		get => _replacedWith;
		internal set {
			_replacedWith = value;

			// todo: kind of icky just modifying the list raw
			if(RequiresUpdateFunction()) {
				SoundAPIAudioManager.liveAudioSourceData.Add(this);
			}
		}
	}

	/// <summary>
	/// Should SoundAPI ignore replacing for this Audio Source?
	/// </summary>
	public bool DisableReplacing { get; set; }

	/// <summary>
	/// Current Context, may be null.
	/// </summary>
	public IContext CurrentContext { get; set; }

	/// <summary>
	/// Volume scale for this Audio Source (mainly for OneShots).
	/// </summary>
	public float VolumeScale { get; set; } = 1.0f;

	internal void Update() {
		if(!RequiresUpdateFunction() || !AudioSourceIsPlaying()) {
			return;
		}

		Debuggers.UpdateEveryFrame?.Log($"success: updating every frame for {Source.name}");

		CurrentContext ??= new DefaultConditionContext(Source);
		SoundInstance sound = ReplacedWith.Sounds.Find(it => it.Evaluate(CurrentContext));
		if(sound == null || !sound.Clip) {
			return;
		}

		if(sound.Clip == Source.clip) {
			return;
		}

		Debuggers.UpdateEveryFrame?.Log("new clip found, swapping!!");

		if(sound.Parent?.Volume.HasValue == true) {
			Source.volume = sound.Parent.Volume.Value * VolumeScale;
			if(Debuggers.AudioSourceAdditionalData != null) {
				string volume = $"{Source.volume}";
				if(Source.volume != sound.Parent.Volume.Value) {
					volume += $" ({sound.Parent.Volume.Value} * {VolumeScale})";
				}
				Debuggers.AudioSourceAdditionalData?.Log($"Changed {Source} (gameobject: {Source.name}) volume to: {volume})");
			}
		}


		float currentTime = Source.time;
		if(currentTime >= sound.Clip.length) {
			Source.Stop(); // TODO: Condition to remember playback time.
			return;
		}
		Source.clip = sound.Clip;

		Source.Play();
		Source.time = currentTime;

		Debuggers.UpdateEveryFrame?.Log("new clip found, swapped");
	}

	bool RequiresUpdateFunction() {
		return ReplacedWith != null && ReplacedWith.UpdateEveryFrame;
	}

	bool AudioSourceIsPlaying() {
		return Source && Source.enabled && Source.isPlaying;
	}

	public static AudioSourceAdditionalData GetOrCreate(AudioSource source) {
		if(SoundAPIAudioManager.audioSourceData.TryGetValue(source, out AudioSourceAdditionalData sourceData)) {
			return sourceData;
		}

		sourceData = new AudioSourceAdditionalData(source);
		if(!sourceData.OriginalClip) // Only set original clip if missing.
			sourceData.OriginalClip = sourceData.RealClip;
		SoundAPIAudioManager.audioSourceData[source] = sourceData;

		Debuggers.AudioSourceAdditionalData?.Log($"created {source.gameObject.name} = {source.m_CachedPtr.ToInt64()}");

		return sourceData;
	}

	internal static bool TryGet(AudioSource source, out AudioSourceAdditionalData data) {
		return SoundAPIAudioManager.audioSourceData.TryGetValue(source, out data);
	}

	public override string ToString() {
		if(Source == null) return base.ToString();
		return $"'{Source.name}' ('{(OriginalClip ? OriginalClip.name : "null")}' -> '{(RealClip ? RealClip.name : "null")}') | {ReplacedWith}";
	}
}