using HarmonyLib;
using Photon.Voice.Windows;
using SoundBoard.Core;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(WindowsAudioInPusher))]
internal class WindowsAudioInPusherPatch
{
    private static SoundEngine? SoundEngine => SoundEngine.Instance;

    [HarmonyPrefix]
    [HarmonyPatch("SetCallback")]
    private static void SetCallback(WindowsAudioInPusher __instance, ref Action<short[]> callback)
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