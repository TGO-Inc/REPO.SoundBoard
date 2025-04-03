using System.Collections.Concurrent;
using HarmonyLib;
using Photon.Voice.Unity;
using SoundBoard.Models.Audio;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(MicWrapper))]
internal class MicWrapperPatch
{
    private static Core.SoundEngine? SoundEngine => Core.SoundEngine.Instance;
    
    [HarmonyPostfix]
    [HarmonyPatch("Read")]
    public static void Read(MicWrapper __instance, float[] buffer, ref bool __result)
    {
        // if (!__result || SoundEngine is null)
        //     return;
        //
        // AudioMixer.MixAudio(buffer, __instance.SamplingRate, __instance.Channels, SoundEngine);
    }
}