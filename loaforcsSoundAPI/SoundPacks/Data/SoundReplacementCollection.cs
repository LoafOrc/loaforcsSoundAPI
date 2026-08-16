using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.Core.Util;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using Newtonsoft.Json;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class SoundReplacementCollection : Conditional, IFilePathAware, IPackData, IRegistrationCallback, IValidatable {
	[JsonConstructor]
	internal SoundReplacementCollection() { }

	public SoundReplacementCollection(SoundPack pack) {
		Pack = pack;
		pack.ReplacementCollections.Add(this);
	}

	internal void AddSoundReplacementGroup(SoundReplacementGroup group) {
		Replacements.Add(group);
	}

	[field: NonSerialized]
	public override SoundPack Pack { get; set; }

	public bool UpdateEveryFrame { get; private set; }


	public bool Synced { get; private set; }

	public List<SoundReplacementGroup> Replacements { get; private set; } = [ ];

	public string FilePath { get; set; } = string.Empty;

	public string RelativePath {
		get {
			if(field.Length == 0) {
				field = LogFormats.FormatFilePath(FilePath);
			}
			return field;
		}
		set;
	} = string.Empty;

	public override void OnRegistered() {
		base.OnRegistered();
		foreach(SoundReplacementGroup group in Replacements) {
			group.OnRegistered();
		}
	}

	public override List<IValidatable.ValidationResult> Validate() {
		List<IValidatable.ValidationResult> results = base.Validate();

		foreach(SoundReplacementGroup group in Replacements) {
			group.Parent = this; // !!! - Setting data while doing validation. If this ever breaks it's here!
			results.AddRange(group.Validate());
		}

		return results;
	}

	public override string ToString() {
		return $"Collection in pack '{Pack.Name}' with #{Replacements.Count} replacements: {RelativePath}";
	}
}