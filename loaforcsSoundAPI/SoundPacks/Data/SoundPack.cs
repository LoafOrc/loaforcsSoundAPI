using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Logging;
using JetBrains.Annotations;
using loaforcsSoundAPI.Core.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements.UIR;
using UnityEngine.Windows.Speech;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class SoundPack : IValidatable, IDeserializationCallback, IFilePathAware {
	// this is very icky because really this should not be referencing newtonsoft json in any way but i could not care less at the moment
	[JsonConstructor]
	internal SoundPack() { }

	[JsonProperty]
	Dictionary<string, JObject> config { get; set; } // handled by json seralization 

	// this too
	internal void Bind(ConfigFile file) {
		if(config == null) return;
		if(config.Count == 0) return;
		loaforcsSoundAPI.Logger.LogDebug("handling config");

		foreach(KeyValuePair<string, JObject> pair in config) {
			string[] sectionData = pair.Key.Split(":");
			string configSection = sectionData[0];
			string configName = sectionData[1];
			JToken defaultValue = pair.Value["default"];
			string description = pair.Value.TryGetValue("description", out JToken value) ? value.ToString() : "no description defined!";

			switch(defaultValue.Type) {
				case JTokenType.Boolean:
					_configData[pair.Key] = file.Bind(configSection, configName, (bool) defaultValue, description).Value;
					break;
				case JTokenType.String:
					_configData[pair.Key] = file.Bind(configSection, configName, (string) defaultValue, description).Value;
					break;
				default:
					throw new NotImplementedException("WHAT");
			}
		}
	}

	public string Name { get; private set; }
	public string GUID => $"soundpack.{Name}"; // todo: probably figure out a better way to do this.

	[CanBeNull]
	public string Version { get; private set; }

	public string PackFolder { get; private set; } // has to be internal as it is set not from a json property but elsewhere, kinda icky

	[field: NonSerialized]
	public List<SoundReplacementCollection> ReplacementCollections { get; private set; } = [ ];

	[field: NonSerialized]
	readonly Dictionary<string, object> _configData = [ ];

	ManualLogSource _logger;

	[field: NonSerialized]
	public ReplacementsRegistry Replacers { get; private set; }

	public ManualLogSource Logger {
		get {
			if(_logger == null) _logger = BepInEx.Logging.Logger.CreateLogSource(GUID);
			return _logger;
		}
	}

	internal bool TryGetConfigValue(string id, out object returnValue) {
		returnValue = default;
		if(!_configData.TryGetValue(id, out object data)) return false;
		returnValue = data;
		return true;
	}

	/// <inheritdoc />
	public List<IValidatable.ValidationResult> Validate() {
		List<IValidatable.ValidationResult> results = [ ];

		if(string.IsNullOrEmpty(Name)) {
			results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, "'name' can not be missing or empty!"));
			return results;
		}

		foreach(char character in Name) {
			if(char.IsLetter(character) || character == '_') continue;
			results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"'name' can not contain special character '{character}'!"));
		}

		if(string.IsNullOrEmpty(Version) && PackLoadingConfig.MetadataSpoofing) results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.WARN, "'version' should not be empty, defaulting to '1.0.0' (needed by MetadataSpoofing config)"));

		if(config == null) return results;

		foreach(KeyValuePair<string, JObject> pair in config) {
			string[] sectionData = pair.Key.Split(":");

			if(sectionData.Length != 2) results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"'{pair.Key}' is not a valid key for config! It must be 'section:name' with exactly one colon!"));

			if(!pair.Value.TryGetValue("default", out JToken defaultValue)) {
				results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"'{pair.Key}' does not have a 'default' value! This is needed to get what type the config is!"));
			} else {
				if(defaultValue.Type != JTokenType.Boolean && defaultValue.Type != JTokenType.String) results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"'{pair.Key}' is of unsupported type: '{defaultValue.Type}'! Only supported types are strings/text or booleans!"));
			}

			if(!pair.Value.ContainsKey("description")) results.Add(new IValidatable.ValidationResult(IValidatable.ResultType.WARN, $"'{pair.Key}' does not have a description."));
		}

		return results;
	}

	public void OnDeserialized() {
		PackFolder = Path.GetDirectoryName(FilePath);
		Replacers = new ReplacementsRegistry(this, "replacers");
	}

	public string FilePath { get; set; }
}