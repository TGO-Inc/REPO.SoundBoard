using HarmonyLib;
// using Photon.Pun;
// using Photon.Voice.Unity;
// using SoundBoard.Sound;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace SoundBoard.Patches;
//
// [HarmonyPatch(typeof(PlayerVoiceChat))]
// public class PlayerVoiceChatPatch
// {
//     public static volatile bool BlockMicrophone = false;
//     
//     [HarmonyPrefix]
//     [HarmonyPatch("Update")]
//     private static void Start(PlayerVoiceChat __instance, ref string ___currentDeviceName, ref bool ___microphoneEnabled, ref bool ___microphoneEnabledPrevious)
//     {
//         // if (BlockMicrophone)
//         // {
//         //     ___microphoneEnabled = false;
//         //     ___microphoneEnabledPrevious = false;
//         //     ___currentDeviceName = "NONE";
//         // }
//     }
// }