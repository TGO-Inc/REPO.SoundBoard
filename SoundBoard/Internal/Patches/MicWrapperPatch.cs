using System.Collections.Concurrent;
using HarmonyLib;
using Photon.Voice.Unity;
using SoundBoard.Core;
using SoundBoard.Helpers;
using SoundBoard.Models.Audio;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(MicWrapper))]
internal class MicWrapperPatch
{
    private static readonly ConcurrentDictionary<MicWrapper, bool> DoAnother = [];
    
    [HarmonyPostfix]
    [HarmonyPatch("Read")]
    public static void Read(MicWrapper __instance, float[] buffer, ref bool __result)
    {
        var pass = __result || (DoAnother.TryGetValue(__instance, out var doAnother) && doAnother);

        if (!pass || SoundEngine.Instance is null || !SoundEngine.Instance.IsAnyAudioPlaying) return;
        
        if (!DoAnother.TryGetValue(__instance, out _))
            DoAnother.TryAdd(__instance, false);
        
        __result = true;
        DoAnother[__instance] = false;
        AudioMixer.MixAudio(buffer, SoundEngine.Instance);

        if (SoundEngine.Instance.RequiredFrameSize <= 0 ||
            SoundEngine.Instance.RequiredFrameSize * 6 < SoundEngine.Instance.AudioBuffer.Count) return;
        
        DoAnother[__instance] = true;
    }
}