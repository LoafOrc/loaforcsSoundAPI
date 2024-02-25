using HarmonyLib;
using loaforcsSoundAPI.Behaviours;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI.Patches;

[HarmonyPatch(typeof(UnityEngine.Object))]
internal static class UnityObjectPatch {
    [HarmonyPostfix, 
        HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object) }),
        HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Transform), typeof(bool) }),
        HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) }),
        HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })
    ]
    internal static void FixPlayOnAwake(ref UnityEngine.Object __result) {
        if (__result is not GameObject) return;
        CheckGameObject(__result as GameObject);
    }

    static void CheckGameObject(GameObject @object) {
        if (@object.TryGetComponent(out AudioSourceReplaceHelper __)) return; // already processed
        AudioSource[] sources = @object.GetComponents<AudioSource>();
        foreach (AudioSource source in sources) {
            if (source.playOnAwake)
                source.Stop();

            AudioSourceReplaceHelper ext = @object.AddComponent<AudioSourceReplaceHelper>();
            ext.source = source;
        }

        foreach (Transform transform in @object.transform) {
            CheckGameObject(transform.gameObject);
        }
    }
}
