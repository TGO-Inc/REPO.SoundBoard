using HarmonyLib;
using Photon.Voice.Unity;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(WebRtcAudioDsp))]
public class WebRtcAudioDspPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("PhotonVoiceCreated", typeof(PhotonVoiceCreatedParams))]
    private static bool PhotonVoiceCreated()
    {
        // Skip the original method and suppress the annoying error log
        return false;
    }
}