using HarmonyLib;
using Photon.Pun;
using SoundBoard.Core;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(PlayerVoiceChat))]
internal class PlayerVoiceChatPatch
{
    private static SoundEngine? SoundEngine => SoundEngine.Instance;
    
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void Start(PlayerVoiceChat __instance, PhotonView ___photonView)
    {
        if (!___photonView.IsMine) return;
        Entry.SoundBoard.RegisterSoundEngine(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("FixedUpdate")]
    public static void FixedUpdate(PlayerVoiceChat __instance, PhotonView ___photonView, ref float ___isTalkingTimer, ref bool ___isTalking)
    {
        if (!___photonView.IsMine) return;

        if (SoundEngine is null || !SoundEngine.IsAnyAudioPlaying)
            return;
        
        ___isTalkingTimer = 1f;
        ___isTalking = true;
    }
}