using HarmonyLib;
using Photon.Voice.Unity;
using SoundBoard.Core;
using SoundBoard.Helpers;
using SoundBoard.Models.Audio;
using UnityEngine;
using ILogger = Photon.Voice.ILogger;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(MicWrapperPusher))]
internal class MicWrapperPusherPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SetCallback")]
    public static void SetCallback(MicWrapperPusher __instance, ref Action<float[]> callback)
    {
        var original = callback;
        callback = buffer =>
        {
            // invoke extension
            if (SoundEngine.Instance is not null && SoundEngine.Instance.IsAnyAudioPlaying)
                AudioMixer.MixAudio(buffer, SoundEngine.Instance);
            
            // invoke original
            original?.Invoke(buffer);
        };
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor, typeof(GameObject), typeof(string), typeof(int), typeof(ILogger))]
    public static void Constructor(MicWrapperPusher __instance, ref int ___channels)
    {
        ___channels = 1;
    }
}