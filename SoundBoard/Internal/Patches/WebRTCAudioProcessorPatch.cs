using HarmonyLib;
using Photon.Voice;
using SoundBoard.Core;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(WebRTCAudioProcessor))]
public class WebRTCAudioProcessorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Process", typeof(short[]))]
    public static void Process(ref short[] __result, short[] buf)
    {
        if (SoundEngine.Instance is not null && SoundEngine.Instance.IsAnyAudioPlaying)
            __result = buf;
    }
}