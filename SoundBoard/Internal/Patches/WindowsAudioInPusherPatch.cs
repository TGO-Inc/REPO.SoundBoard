using HarmonyLib;
using Photon.Voice.Windows;
using SoundBoard.Core;
using SoundBoard.Models.Audio;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(WindowsAudioInPusher))]
internal class WindowsAudioInPusherPatch
{
    private static SoundEngine? SoundEngine => SoundEngine.Instance;
    
    [HarmonyPrefix]
    [HarmonyPatch("SetCallback")]
    public static void SetCallback(WindowsAudioInPusher __instance, ref Action<short[]> callback)
    {
        var original = callback;
        callback = buffer =>
        {
            // if (SoundEngine is not null && SoundEngine.IsAnyAudioPlaying)
            //     AudioMixer.MixAudio(buffer, SoundEngine);
            
            original?.Invoke(buffer);
        };
    }
}