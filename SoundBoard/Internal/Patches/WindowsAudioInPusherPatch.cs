using HarmonyLib;
using Photon.Voice.Windows;
using SoundBoard.Core;
using SoundBoard.Helpers;
using SoundBoard.Models.Audio;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(WindowsAudioInPusher))]
internal class WindowsAudioInPusherPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SetCallback")]
    public static void SetCallback(WindowsAudioInPusher __instance, ref Action<short[]> callback)
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
}