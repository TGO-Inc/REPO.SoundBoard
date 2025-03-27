using HarmonyLib;
using Photon.Voice.Unity;
using SoundBoard.Sound;

namespace SoundBoard.Patches;

[HarmonyPatch(typeof(MicWrapperPusher))]
internal class MicWrapperPusherPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SetCallback")]
    public static void SetCallback(MicWrapperPusher __instance, ref Action<float[]> callback)
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
        
        Settings.Instance.Logger.LogInfo("MicWrapperPusher SetCallback");
    }
}