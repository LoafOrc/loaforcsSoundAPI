using System;
using System.Collections.Generic;
using System.Linq;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks;

static class SoundPackDataHandler {
	static List<SoundPack> _loadedPacks = [];

	internal static IReadOnlyList<SoundPack> LoadedPacks => _loadedPacks.AsReadOnly();

	internal static Dictionary<string, List<SoundReplacementGroup>> SoundReplacements = [];


	// this seems kinda in-efficent but i dont really care
	internal static Dictionary<string, Func<Condition>> conditionFactories = new Dictionary<string, Func<Condition>>();
	internal static List<AudioClip> allLoadedClips = new List<AudioClip>();

	internal static void Register(string id, Func<Condition> factory) {
		conditionFactories[id] = factory;
	}

	public static Condition CreateCondition(string id) {
		if(conditionFactories.TryGetValue(id, out Func<Condition> factory)) {
			return factory();
		}

		return new InvalidCondition(id);
	}

	internal static void AddLoadedPack(SoundPack pack) {
		_loadedPacks.Add(pack);
	}

	internal static void AddReplacement(SoundReplacementGroup group) {
		foreach(string match in group.Matches) {
			string[] splitMatch = match.Split(':', StringSplitOptions.RemoveEmptyEntries);
			if(splitMatch.Length == 0) continue;
			string clipName = splitMatch.Last();
			if(!SoundReplacements.TryGetValue(clipName, out List<SoundReplacementGroup> existingGroups)) {
				existingGroups = [];
			}

			if(existingGroups.Contains(group)) continue;
			if(existingGroups.Count == 0 || existingGroups.Last().Priority >= group.Priority) {
				existingGroups.Add(group);
			} else {
				for(int i = existingGroups.Count - 1; i >= 0; i--) {
					if(existingGroups[i].Priority <= group.Priority) {
						existingGroups.Insert(i, group);
						break;
					}
				}
			}
			SoundReplacements[clipName] = existingGroups;
		}
	}

	internal static void RemoveReplacement(SoundReplacementGroup group) {
		foreach(string match in group.Matches) {
			string[] splitMatch = match.Split(':', StringSplitOptions.RemoveEmptyEntries);
			if(splitMatch.Length == 0) continue;
			string clipName = splitMatch.Last();

			SoundReplacements[clipName].Remove(group);
		}
	}

	internal static List<SoundReplacementGroup> GetReplacements(string match) {
		return SoundReplacements.TryGetValue(match, out List<SoundReplacementGroup> groups) ? groups : [];
	}
}