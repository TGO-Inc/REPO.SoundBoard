using System.Reflection;
using HarmonyLib;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(Type))]
public class TypePatch
{
    [HarmonyPrefix]
    [HarmonyPatch("GetMethods", [])]
    public static bool GetMethods(Type __instance, ref MethodInfo[] __result)
    {
        try
        {
            // Use default binding flags
            const BindingFlags defaultFlags = BindingFlags.Public | BindingFlags.Instance |
                                              BindingFlags.NonPublic | BindingFlags.Static;
        
            // Call the overload with binding flags
            __result = __instance.GetMethods(defaultFlags);
            return false; // Skip original method execution
        }
        catch (Exception ex)
        {
            Entry.LogSource.LogError($"Error getting methods: {ex.Message}");
            __result = []; // Return empty array instead of throwing
            return false; // Skip original method
        }
    }
}