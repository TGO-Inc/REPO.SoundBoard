using System.Collections.Concurrent;
using HarmonyLib;
using Photon.Voice.Unity;
using SoundBoard.Sound;
using SoundBoard.Sound.Models;

namespace SoundBoard.Patches;

[HarmonyPatch(typeof(MicWrapper))]
internal class MicWrapperPatch
{
    private static readonly ConcurrentDictionary<MicWrapper, bool> DoAnother = [];
    
    [HarmonyPostfix]
    [HarmonyPatch("Read")]
    public static void Read(MicWrapper __instance, float[] buffer, ref bool __result)
    {
        if ((__result || (DoAnother.TryGetValue(__instance, out var again) && again)) && CustomSoundManager.Instance is not null)
        {
            if (!DoAnother.TryGetValue(__instance, out var doAnother))
                DoAnother.TryAdd(__instance, true);
            
            AudioHelper.MixAudio(buffer, CustomSoundManager.Instance);

            // Not enough data in the audio buffer, make sure to "queue" another audio read.
            if (CustomSoundManager.Instance.AudioBuffer.Count < CustomSoundManager.Instance.RequiredFrameSize * 10)
            {
                __result = true;
                DoAnother[__instance] = true;
                return;
            }
            
            // Reset the repeat flag.
            if (!__result)
            {
                DoAnother[__instance] = false;
                __result = true;
            }
            else
            {
                DoAnother[__instance] = true;
            }
        }
    }
}