using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using loaforcsSoundAPI.Core.Networking;
using loaforcsSoundAPI.SoundPacks;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using loaforcsSoundAPI.Core.Util.Extensions;
using UnityEngine;
using UnityEngine.Networking;
using loaforcsSoundAPI.SoundPacks.AudioClipLoading;

namespace loaforcsSoundAPI;

/// <summary>
/// Main SoundAPI functionality
/// </summary>
public static class SoundAPI {
	/// <summary>
	/// BepInEx plugin GUID for dependency. It is fine to reference as a soft-dependency.
	/// </summary>
	public const string PLUGIN_GUID = MyPluginInfo.PLUGIN_GUID;

	internal static NetworkAdapter CurrentNetworkAdapter { get; private set; }

	/// <summary>
	/// Loads an audio clip from a given file. 
	/// </summary>
	/// <exception cref="NotImplementedException">Thrown if an unsupported audio file extension is passed.</exception>
	/// <exception cref="FileNotFoundException">File not found</exception>
	/// <remarks></remarks>
	/// <returns>AudioClip</returns>
	public static async Task<AudioClip> LoadAudioFileAsync(string fullPath) {
		// todo: this is effectively duplicated code from the soundpack load pipeline, look at combining the code from there into this.

		if(!File.Exists(fullPath)) {
			throw new FileNotFoundException($"'{fullPath}' not found.");
		}

		if(!IAudioClipLoader.audioExtensions.ContainsKey(Path.GetExtension(fullPath))) {
			throw new NotImplementedException($"Audio file extension: '{Path.GetExtension(fullPath)}' is not implemented.");
		}

		UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, IAudioClipLoader.audioExtensions[Path.GetExtension(fullPath)]);
		await request.SendWebRequest();

		AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
		request.Dispose();
		return clip;
	}

	/// <summary>
	/// Finds all types marked with [SoundAPICondition] and registers them with a default factory method.
	/// </summary>
	/// <seealso cref="SoundAPIConditionAttribute"/>
	/// <seealso cref="Condition{ContextType}"/>
	/// <param name="assembly">Assembly to search</param>
	public static void RegisterAll(Assembly assembly) {
		foreach(Type type in assembly.GetLoadableTypes()) {
			if(type.IsNested) {
				continue;
			}

			foreach(SoundAPIConditionAttribute conditionAttribute in type.GetCustomAttributes<SoundAPIConditionAttribute>()) {
				if(!typeof(Condition).IsAssignableFrom(type)) {
					loaforcsSoundAPI.Logger.LogError($"Condition: '{type.FullName}' has been marked with [SoundAPICondition] but does not extend Condition!");
					continue;
				}

				ConstructorInfo info = type.GetConstructor([ ]);
				if(info == null) {
					loaforcsSoundAPI.Logger.LogError(
						$"Condition: '{type.FullName}' has no valid constructor! It must have a constructor with no parameters! " +
						$"If you need extra parameters do not mark it with [SoundAPICondition] and register it manually."
					);
					continue;
				}

				RegisterCondition(conditionAttribute.ID, () => {
					if(conditionAttribute.IsDeprecated) { // todo: change this to be like InvalidCondition so that it can correctly trigger during validation
						if(conditionAttribute.DeprecationReason == null) {
							loaforcsSoundAPI.Logger.LogWarning($"Condition: '{conditionAttribute.ID}' is deprecated and may be removed in future.");
						} else {
							loaforcsSoundAPI.Logger.LogWarning($"Condition: '{conditionAttribute.ID}' is deprecated. {conditionAttribute.DeprecationReason}");
						}
					}

					return (Condition) info.Invoke([ ]);
				});
			}
		}
	}

	// this seems a bit dumb because it just surfaces an internal method, but i want to keep all public methods in SoundAPI
	/// <summary>
	/// Register a condition
	/// </summary>
	/// <seealso cref="Condition{ContextType}"/>
	/// <param name="id">A unique ID for this condition</param>
	/// <param name="factory">The method to create a condition per instance.</param>
	public static void RegisterCondition(string id, Func<Condition> factory) {
		SoundPackDataHandler.Register(id, factory);
	}


	/// <summary>
	/// Registers a network adapter.
	/// </summary>
	/// <param name="adapter"></param>
	public static void RegisterNetworkAdapter(NetworkAdapter adapter) {
		CurrentNetworkAdapter = adapter;
		loaforcsSoundAPI.Logger.LogInfo($"Registered network adapter: '{CurrentNetworkAdapter.Name}'");
		CurrentNetworkAdapter.OnRegister();
	}

	/// <summary>
	/// Creates an exact copy of an audio source onto another game object.
	/// </summary>
	/// <param name="source">The source to be copied</param>
	/// <param name="target">The destination GameObject</param>
	/// <param name="flags">Additional flags for when copying the audio source</param>
	public static AudioSource CopyAudioSource(AudioSource source, GameObject target, AudioSourceCopyFlags flags = default) {
		// todo: surely there is going to be a cleaner way to do this??
		AudioSource newSource = target.AddComponent<AudioSource>();
		newSource.clip = source.clip;
		newSource.loop = (flags & AudioSourceCopyFlags.DontCopyLoop) == 0 && source.loop;
		newSource.mute = source.mute;
		newSource.pitch = source.pitch;
		newSource.outputAudioMixerGroup = source.outputAudioMixerGroup;
		newSource.priority = source.priority;
		newSource.spatialize = (flags & AudioSourceCopyFlags.DontCopySpatialize) == 0 && source.spatialize;
		newSource.spread = source.spread;
		newSource.volume = source.volume;
		newSource.bypassEffects = source.bypassEffects;
		newSource.dopplerLevel = source.dopplerLevel;
		newSource.maxDistance = source.maxDistance;
		newSource.minDistance = source.minDistance;
		newSource.panStereo = source.panStereo;
		newSource.rolloffMode = source.rolloffMode;
		newSource.spatialBlend = source.spatialBlend;
		newSource.bypassReverbZones = source.bypassReverbZones;
		newSource.ignoreListenerPause = source.ignoreListenerPause;
		newSource.ignoreListenerVolume = source.ignoreListenerVolume;
		newSource.playOnAwake = (flags & AudioSourceCopyFlags.DontCopyPlayOnAwake) == 0 && source.playOnAwake; // double negative isn't great

		newSource.reverbZoneMix = source.reverbZoneMix;
		newSource.spatializePostEffects = source.spatializePostEffects;
		newSource.velocityUpdateMode = source.velocityUpdateMode;

		newSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, source.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
		newSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, source.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
		newSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, source.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
		newSource.SetCustomCurve(AudioSourceCurveType.Spread, source.GetCustomCurve(AudioSourceCurveType.Spread));

		return newSource;
	}
}