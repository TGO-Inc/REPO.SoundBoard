using HarmonyLib;
using UnityEngine;

namespace SoundBoard.Patches;

[HarmonyPatch(typeof(Microphone))]
public class MicrophonePatch
{
    public static volatile bool BlockMicrophone = false;
    
    [HarmonyPrefix]
    [HarmonyPatch("Start", typeof(string), typeof(bool), typeof(int), typeof(int))]
    private static bool Start(ref AudioClip __result, ref int frequency, ref int lengthSec)
    {
        if (!BlockMicrophone) return true;
        
        __result = AudioClip.Create("", lengthSec, 1, frequency, false);
        return false;

    }
}