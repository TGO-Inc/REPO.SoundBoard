using HarmonyLib;
using Photon.Voice.Unity;
using SoundBoard.Core;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(MicWrapper))]
internal class MicWrapperPatch
{
    private static SoundEngine? SoundEngine => SoundEngine.Instance;

    [HarmonyPostfix]
    [HarmonyPatch("Read")]
    private static void Read(MicWrapper __instance, float[] buffer, ref bool __result)
    {
        // if (!__result || SoundEngine is null)
        //     return;
        //
        // AudioMixer.MixAudio(buffer, __instance.SamplingRate, __instance.Channels, SoundEngine);
    }
}