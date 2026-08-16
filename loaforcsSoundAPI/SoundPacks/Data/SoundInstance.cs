using System;
using System.Collections.Generic;
using System.IO;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.AudioClipLoading;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using Newtonsoft.Json;
using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class SoundInstance : Conditional, IValidatable {
	[JsonConstructor]
	internal SoundInstance() { }

	public SoundInstance(SoundReplacementGroup parent, int weight, AudioClip clip) {
		Parent = parent;
		Weight = weight;
		Clip = clip;
		parent.AddSoundReplacement(this);
	}

	[field: NonSerialized]
	public SoundReplacementGroup Parent { get; internal set; }

	public string Sound { get; private set; }

	public int Weight { get; private set; }

	internal string FullPath => Path.Combine(Pack.PackFolder, "sounds", Sound);

	[field: NonSerialized]
	public AudioClip Clip {
		get;
		internal set {
			field = value;
			if(field == null) throw new InvalidOperationException($"Tried to set a null or missing clip for sound '{Sound}'!");
			field.name = Sound[(Sound.LastIndexOf('/') + 1)..Sound.LastIndexOf('.')];
		}
	}

	public override List<IValidatable.ValidationResult> Validate() {
		List<IValidatable.ValidationResult> results = base.Validate();

		if(!File.Exists(FullPath)) {
			results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Sound '{Sound}' couldn't be found or doesn't exist!"));
		} else if(!IAudioClipLoader.audioExtensions.ContainsKey(Path.GetExtension(Sound))) {
			results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Audio type: '{Path.GetExtension(Sound)}' is not supported!"));
		}

		return results;
	}

	public override SoundPack Pack {
		get => Parent.Pack;
		set {
			if(Parent.Pack != null) throw new InvalidOperationException("Pack has already been set.");
			Parent.Pack = value;
		}
	}

	public override string ToString() {
		return $"Sound at '{Sound}' from pack '{Pack.Name}' with a weight of {Weight}\n\tClip name: {((Clip != null) ? Clip.name : "null")}";
	}
}