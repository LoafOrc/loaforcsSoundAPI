using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace loaforcsSoundAPI.Core.JSON;

/// <summary>
/// Handles SoundAPI's JSON loading operations.
/// </summary>
public static class JSONDataLoader {
	static readonly JsonSerializerSettings _settings = new JsonSerializerSettings {
		ContractResolver = new IncludePrivatePropertiesContractResolver(),
		Converters = [
			new MatchesJSONConverter(),
			new ConditionConverter(),
			new ContentReferenceConverter(),
			new RangeOperatorConverter()
		]
	};

	/// <summary>
	/// Load JSON file as specified type T
	/// </summary>
	/// <param name="path">File path</param>
	/// <typeparam name="T">Type</typeparam>
	/// <returns>An instance of T or null if an error occured while loading</returns>
	public static T LoadFromFile<T>(string path) {
		string input = File.ReadAllText(path);
		try {
			T result = JsonConvert.DeserializeObject<T>(input, _settings);

			if(result is IFilePathAware dataFile) {
				dataFile.FilePath = path;
			}

			if(result is Conditional conditional && conditional.Condition != null) {
				conditional.Condition.Parent = conditional;
			}

			if(result is IDeserializationCallback callback) {
				callback.OnDeserialized();
			}

			return result;
		} catch(JsonReaderException exception) {
			loaforcsSoundAPI.Logger.LogError($"Failed to read json file: 'plugins{Path.DirectorySeparatorChar}{Path.GetRelativePath(Paths.PluginPath, path)}'");
			loaforcsSoundAPI.Logger.LogError(exception.Message);

			// handle showing context around the error.
			string[] lines = input.Split("\n");

			int minLeadingSpaces = int.MaxValue;

			// Count leading spaces
			for(int i = Mathf.Max(0, exception.LineNumber - 3); i < Mathf.Min(lines.Length, exception.LineNumber + 3); i++) {
				int leadingSpaces = lines[i].TakeWhile(char.IsWhiteSpace).Count();
				minLeadingSpaces = Mathf.Min(minLeadingSpaces, leadingSpaces);
			}

			for(int i = Mathf.Max(0, exception.LineNumber - 3); i < Mathf.Min(lines.Length, exception.LineNumber + 3); i++) {
				string lineContent = $"{(i + 1).ToString(),-5}|  " + lines[i][Mathf.Min(lines[i].Length, minLeadingSpaces)..].TrimEnd();

				if(i + 1 == exception.LineNumber) {
					lineContent += " // <- HERE";
				}

				loaforcsSoundAPI.Logger.LogError(lineContent);
			}
		}

		return default;
	}

	class MatchesJSONConverter : JsonConverter {
		public override bool CanConvert(Type objectType) {
			return objectType == typeof(List<string>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			JToken token = JToken.Load(reader);
			if(token is not JArray array) {
				array = [token];
			}

			return array.ToObject<List<string>>();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			serializer.Serialize(writer, value);
		}
	}

	class IncludePrivatePropertiesContractResolver : DefaultContractResolver {
		internal IncludePrivatePropertiesContractResolver() {
			NamingStrategy = new SnakeCaseNamingStrategy();
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			if(!property.Writable && member is PropertyInfo propInfo) {
				property.Writable = propInfo.GetSetMethod(true) != null;
			}

			return property;
		}
	}

	class ConditionConverter : JsonConverter<Condition> {
		public override Condition ReadJson(JsonReader reader, Type objectType, Condition existingValue, bool hasExistingValue, JsonSerializer serializer) {
			// first read a JToken to determine if its a boolean or a condition
			JToken token = JToken.Load(reader);

			if(token.Type == JTokenType.Boolean) {
				bool value = token.Value<bool>();
				return value ? ConstantCondition.TRUE : ConstantCondition.FALSE;
			}

			if(token.Type == JTokenType.Object) {
				JObject jsonObject = (JObject) token;

				// get the "type" field to determine which condition class to use
				string conditionType = jsonObject["type"]?.ToString();

				if(string.IsNullOrEmpty(conditionType)) return new InvalidCondition(null);

				Condition condition = SoundPackDataHandler.CreateCondition(conditionType);
				if(condition == null) return null;

				serializer.Populate(jsonObject.CreateReader(), condition);

				return condition;
			}

			IJsonLineInfo lineInfo = reader as IJsonLineInfo;
			throw new JsonReaderException($"'{token}' is not valid for a condition", reader.Path, lineInfo?.LineNumber ?? 0, lineInfo?.LinePosition ?? 0, null);
		}

		public override void WriteJson(JsonWriter writer, Condition value, JsonSerializer serializer) {
			throw new NotImplementedException("no.");
		}
	}

	class ContentReferenceConverter : JsonConverter {
		static Dictionary<Type, Dictionary<string, ContentReference>> _referenceCache = [];

		public ContentReferenceConverter() {
			SoundPackLoadPipeline.OnFinishedPipeline += static () => _referenceCache = null;
		}

		public override bool CanConvert(Type objectType) {
			TypeInfo typeInfo = objectType.GetTypeInfo();

			if(typeof(ContentReference).IsAssignableFrom(typeInfo)) return true; // Check for ContentReference type.
			if(typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(List<>)) { // Check for a List of ContentReference type.
				TypeInfo listTypeInfo = typeInfo.GetGenericArguments()[0].GetTypeInfo();
				if(typeof(ContentReference).IsAssignableFrom(listTypeInfo)) return true;
			}

			return false;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			JToken root = JToken.Load(reader);

			TypeInfo typeInfo = objectType.GetTypeInfo();
			if(typeof(ContentReference).IsAssignableFrom(typeInfo)) { // Check for ContentReference type.
				return GetOrCreateReference($"{root}", typeInfo);
			}

			if(root is not JArray array) {
				array = [root];
			}

			TypeInfo listTypeInfo = typeInfo.GetGenericArguments()[0].GetTypeInfo();
			if(typeof(ContentReference).IsAssignableFrom(listTypeInfo)) { // Check for a List of ContentReference type.
				IList genericList = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(listTypeInfo), array.Count);

				foreach(JToken token in array) {
					genericList.Add(GetOrCreateReference($"{token}", listTypeInfo));
				}

				return genericList;
			}

			IJsonLineInfo lineInfo = reader as IJsonLineInfo;
			throw new JsonReaderException($"'{root}' is not valid for a condition", reader.Path, lineInfo?.LineNumber ?? 0, lineInfo?.LinePosition ?? 0, null);
		}

		static ContentReference GetOrCreateReference(string input, Type referenceType) {
			if(!_referenceCache.TryGetValue(referenceType, out Dictionary<string, ContentReference> tokenDictionary)) {
				tokenDictionary = [];
				_referenceCache.Add(referenceType, tokenDictionary);
			}

			if(!tokenDictionary.TryGetValue(input, out ContentReference reference)) {
				reference = (ContentReference) Activator.CreateInstance(referenceType, input);
				tokenDictionary.Add(input, reference);
			}
			reference.Uses++;

			return reference;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			throw new NotImplementedException("no.");
		}
	}

	class RangeOperatorConverter : JsonConverter {
		static Dictionary<Type, Dictionary<string, RangeOperator>> _operatorCache = [];

		public RangeOperatorConverter() {
			SoundPackLoadPipeline.OnFinishedPipeline += static () => _operatorCache = null;
		}

		public override bool CanConvert(Type objectType) {
			return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(RangeOperator<>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			JToken root = JToken.Load(reader);

			TypeInfo operatorType = objectType.GetTypeInfo().GetGenericArguments()[0].GetTypeInfo();
			RangeOperator rangeOperator = GetOrCreateOperator($"{root}", operatorType, existingValue);
			if(rangeOperator != null) {
				rangeOperator.Uses++;

				return rangeOperator;
			}

			IJsonLineInfo lineInfo = reader as IJsonLineInfo;
			throw new JsonReaderException($"'{root}' is not valid for a range operator", reader.Path, lineInfo?.LineNumber ?? 0, lineInfo?.LinePosition ?? 0, null);
		}

		static RangeOperator GetOrCreateOperator(string input, Type operatorType, object operatorLimits) {
			if(!_operatorCache.TryGetValue(operatorType, out Dictionary<string, RangeOperator> tokenDictionary)) {
				tokenDictionary = [];
				_operatorCache.Add(operatorType, tokenDictionary);
			}

			if(!tokenDictionary.TryGetValue(input, out RangeOperator rangeOperator)) {
				rangeOperator = (RangeOperator) Activator.CreateInstance(typeof(RangeOperator<>).MakeGenericType(operatorType), [input, operatorLimits]);

				List<IValidatable.ValidationResult> validationResults = rangeOperator.Validate();
				bool success = IValidatable.LogAndCheckValidationResult($"range operator '{input}'", validationResults, loaforcsSoundAPI.Logger);

				tokenDictionary[input] = success ? rangeOperator : null;
			}

			return rangeOperator;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			throw new NotImplementedException("no.");
		}
	}
}