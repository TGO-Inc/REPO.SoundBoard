using System.Collections.Concurrent;
using HarmonyLib;
using Photon.Voice;
using Photon.Voice.Unity;

namespace SoundBoard.Patches;

[HarmonyPatch(typeof(Recorder))]
public class RecorderPatch
{
    public static ConcurrentDictionary<Recorder, LocalVoice> LocalVoiceAudio = [];
    
    [HarmonyPostfix]
    [HarmonyPatch("CreateLocalVoiceAudioAndSource")]
    public static void CreateLocalVoiceAudioAndSource(Recorder __instance, LocalVoice __voice)
    {
        LocalVoiceAudio.TryAdd(__instance, __voice);
        // __instance.voiceAudio
    }
}