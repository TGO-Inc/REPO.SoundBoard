using System.Collections.Concurrent;
using HarmonyLib;
using Photon.Voice.Unity;
using Photon.Voice.Windows;
using SoundBoard.Sound;
using SoundBoard.Sound.Models;

namespace SoundBoard.Patches;

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
            if (CustomSoundManager.Instance is not null)
                AudioHelper.MixAudio(buffer, CustomSoundManager.Instance);
            
            // invoke original
            original?.Invoke(buffer);
        };
        
        Settings.Instance.Logger.LogInfo("WindowsAudioInPusher SetCallback");
    }
}