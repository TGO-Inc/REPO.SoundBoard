using System.Reflection;
using HarmonyLib;

namespace SoundBoard.Patches;

[HarmonyPatch(typeof(Type))]
internal class TypePatch
{
    [HarmonyPrefix]
    [HarmonyPatch("GetMethods", argumentTypes: [])]
    public static bool GetMethods(Type __instance, ref MethodInfo[] __result)
    {
        try
        {
            __result = __instance.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        catch (Exception ex)
        {
            Settings.Instance!.Logger.LogError(ex);
            __result = [];
        }

        // Skip original method
        return false;
    }
}