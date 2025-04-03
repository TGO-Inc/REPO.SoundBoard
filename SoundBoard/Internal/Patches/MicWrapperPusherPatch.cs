using System.Collections.Concurrent;
using HarmonyLib;
using Photon.Voice.Unity;
using SoundBoard.Core;
using SoundBoard.Models.Audio;
using UnityEngine;
using ILogger = Photon.Voice.ILogger;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(MicWrapperPusher))]
internal class MicWrapperPusherPatch
{
    private static SoundEngine? SoundEngine => SoundEngine.Instance;
    private static readonly ConcurrentDictionary<MicWrapperPusher, int> OldChannels = [];
    
    [HarmonyPrefix]
    [HarmonyPatch("SetCallback")]
    private static void SetCallback(MicWrapperPusher __instance, ref Action<float[]> callback)
    {
        var original = callback;
        callback = buffer =>
        {
            if (SoundEngine is null || !OldChannels.TryGetValue(__instance, out var channels)) return;
            
            // var resizedBuffer = new float[buffer.Length/channels];
            // for (var i = 0; i < buffer.Length; i += channels)
            //     resizedBuffer[i / channels] += buffer[i] / channels;
            
            if (SoundEngine.IsAnyAudioPlaying)
                AudioMixer.MixAudio(buffer, __instance.SamplingRate, channels, SoundEngine);

            // invoke original
            original?.Invoke(buffer);
        };
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor, typeof(GameObject), typeof(string), typeof(int), typeof(ILogger))]
    private static void Constructor(MicWrapperPusher __instance, ref int ___channels)
    {
        OldChannels.TryAdd(__instance, ___channels);
        // var field = __instance.GetType().GetField("onRead");
        // var onRead = field.GetValue(__instance);
        // var readType = onRead.GetType();
        // var ary = readType.GetField("frame2");
        // ary.SetValue(onRead, new float[1024]);
        // var oldMethod = readType.GetMethod("OnAudioFilterRead");
        // ___channels = 1;
    }
}