using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace loaforcsSoundAPI.Patcher;

[HarmonyPatch(typeof(Chainloader))]
static class ChainloaderPatch {
    [HarmonyPatch(nameof(Chainloader.Initialize)), HarmonyPostfix]
    public static void ChainloaderInitializePost() {
        MethodInfo startInfo = typeof(Chainloader).GetMethod(nameof(Chainloader.Start), BindingFlags.Static | BindingFlags.Public);
        MethodInfo postfix = typeof(LoaforcsSoundAPIPatcher).GetMethod(nameof(LoaforcsSoundAPIPatcher.OnChainloaderFinish), BindingFlags.Static | BindingFlags.NonPublic);

        if(startInfo == null || postfix == null) return;
        LoaforcsSoundAPIPatcher.Instance.Harmony.Patch(startInfo, postfix: new HarmonyMethod(postfix, Priority.First));
    }
}
