using System.Reflection;
using HarmonyLib;

namespace SoundBoard.Patches;

// Patch for Type.GetMethods() with no parameters
[HarmonyPatch(typeof(Type))]
public class TypePatch
{
    [HarmonyPrefix]
    [HarmonyPatch("GetMethods", argumentTypes: [])]
    public static bool GetMethods(Type __instance, ref MethodInfo[] __result)
    {
        try
        {
            // Call original method safely
            __result = __instance.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        catch (Exception ex)
        {
            // Log error and return empty array if exception occurs
            // Console.WriteLine($"Error calling GetMethods() on type {__instance.FullName}: {ex.Message}");
            __result = Array.Empty<MethodInfo>(); // Return empty if error occurs
        }

        // Skip original method
        return false;
    }
}

// // Patch for Type.GetMethods(BindingFlags)
// [HarmonyPatch(typeof(Type), "GetMethods", typeof(BindingFlags))]
// public class GetMethodsWithBindingFlagsPatch
// {
//     [HarmonyPrefix]
//     public static bool Prefix(Type __instance, ref MethodInfo[] __result, BindingFlags bindingAttr)
//     {
//         try
//         {
//             // Call original method safely with BindingFlags
//             __result = __instance.GetMethods(bindingAttr);
//         }
//         catch (Exception ex)
//         {
//             // Log error and return empty array if exception occurs
//             // Console.WriteLine($"Error calling GetMethods(BindingFlags) on type {__instance.FullName}: {ex.Message}");
//             __result = Array.Empty<MethodInfo>(); // Return empty if error occurs
//         }
//
//         // Skip original method
//         return false;
//     }
// }