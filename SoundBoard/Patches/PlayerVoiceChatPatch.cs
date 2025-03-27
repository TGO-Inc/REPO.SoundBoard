using HarmonyLib;
using SoundBoard.Sound;

namespace SoundBoard.Patches;

[HarmonyPatch(typeof(PlayerVoiceChat))]
internal class PlayerVoiceChatPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void Awake(PlayerVoiceChat __instance)
    {
        SoundBoard.Instance.Init(__instance);
    }
}