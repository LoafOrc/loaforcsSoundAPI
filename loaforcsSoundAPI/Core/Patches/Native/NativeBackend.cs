using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace loaforcsSoundAPI.Core.Patches.Native;

// Thanks to zaggy for helping with a lot of this! (especially with the GetScriptingWrapper stuff)
static class NativeBackend {
	internal static bool Enabled { get; private set; }
	internal static readonly IntPtr BaseAddress = GetUnityPlayerModule().BaseAddress;
	internal static RuntimePlatform Platform => Application.platform;

	// todo: this should probably be determined by json files or something?
	static NativeBackendSettings[] _allSettings = [
		new NativeBackendSettings("2022.3.9f1", new NativeOffsets(
			0xB9FBC0
		)), // Lethal Company pre-v73
		new NativeBackendSettings("2022.3.62f2", new NativeOffsets(
			0xbccfe0,
			0xbd05b0
		)), // Lethal Company post-v73
		new NativeBackendSettings("2022.3.60f1", new NativeOffsets(
			0x0bcbee0
		)), // Test project
		new NativeBackendSettings("6000.3.15f1", new NativeOffsets(
			0x0c10750
		)) // PEAK
	];

	[UsedImplicitly]
	static List<NativeDetour> _allDetours = [ ];

	internal static bool TryGetSettings(out NativeBackendSettings settings) {
		settings = _allSettings.FirstOrDefault(it => it.CurrentVersionMatches);
		return settings != null;
	}

	internal static IEnumerable<string> SupportedUnityVersions() {
		return _allSettings.Select(it => it.UnityVersion);
	}

	static void Init(NativeBackendSettings settings) {
		Enabled = true;
		AudioSourceNativePatch.Init(settings.WindowsReleaseOffsets);
	}

	internal static bool TryInit() {
		if(!TryGetSettings(out NativeBackendSettings settings)) {
			loaforcsSoundAPI.Logger.LogWarning($"Native backend is not yet supported for unity version: {Application.unityVersion}");
			return false;
		}

		if(Platform != RuntimePlatform.WindowsPlayer) {
			loaforcsSoundAPI.Logger.LogWarning("Native backend is not yet supported on other platforms than Windows. (Proton, etc. may be a workaround?)");
			return false;
		}

		Init(settings);
		return true;
	}

	internal static T PatchNative<T>(int offset, T patch) where T : Delegate {
		IntPtr funAddress = BaseAddress + offset;
		IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(patch);

		NativeDetour detour = new NativeDetour(funAddress, hookPtr);
		_allDetours.Add(detour);
		return detour.GenerateTrampoline<T>();
	}

	static ProcessModule GetUnityPlayerModule() {
		ProcessModuleCollection modules = Process.GetCurrentProcess().Modules;
		for(int i = 0; i < modules.Count; i++) {
			ProcessModule module = modules[i];
			if(module.ModuleName.Contains("UnityPlayer")) {
				return module;
			}
		}

		return null;
	}

	internal static unsafe int GetInstanceID(IntPtr obj) {
		ref int offset = ref Object.OffsetOfInstanceIDInCPlusPlusObject;
		if(offset == -1) {
			offset = Object.GetOffsetOfInstanceIDInCPlusPlusObject();
		}

		return *(int*) (obj + offset);
	}

	internal static unsafe IntPtr GetGCHandle(IntPtr obj) {
		int offset = 0x18; // todo: magic number, does it change?

		return *(IntPtr*) (obj + offset);
	}

	internal static T GetScriptingWrapper<T>(IntPtr self) where T : Object {
		if(GetGCHandle(self) != IntPtr.Zero) { // faster way to do it
			IntPtr handlePtr = GetGCHandle(self);
			GCHandle handle = GCHandle.FromIntPtr(handlePtr);
			return (T) handle.Target;
		}

		// backup. used usually by POA
		Debuggers.NativeBackend?.Log("gc handle was zero");
		int instanceID = GetInstanceID(self);
		Debuggers.NativeBackend?.Log($"instance id = {instanceID}");
		Object obj = Resources.InstanceIDToObject(instanceID);
		Debuggers.NativeBackend?.Log($"obj = {obj}");

		return (T) obj;
	}
}