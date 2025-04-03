using HarmonyLib;
using Photon.Voice;
using SoundBoard.Core;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(WebRTCAudioProcessor))]
internal class WebRTCAudioProcessorPatch
{
    private static SoundEngine? SoundEngine => SoundEngine.Instance;
    
    [HarmonyPostfix]
    [HarmonyPatch("Process", typeof(short[]))]
    private static void Process(ref short[] __result, short[] buf)
    {
        if (SoundEngine is not null && SoundEngine.IsAnyAudioPlaying)
            __result = buf;
    }
}