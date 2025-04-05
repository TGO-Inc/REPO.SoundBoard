using Photon.Voice;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using SoundBoard.Models.Collections;
using UnityEngine;

namespace SoundBoard.Core;

/// <summary>
///     The sound engine only exists when PlayerVoiceChat is active.
/// </summary>
public class SoundEngine : MonoBehaviour
{
    public readonly FastMemory MemManager = new();

    private Recorder? _recorder;
    public static SoundEngine? Instance { get; private set; }
    private static SoundBoard SoundBoard => Entry.SoundBoard;

    /// <summary>
    ///     Checks all static sources for any active audio.
    /// </summary>
    public static bool IsAnyAudioPlaying => SoundBoard.Sounds.Any(sound => sound.IsPlaying);

    private void FixedUpdate()
    {
        if (Instance is null || _recorder is null)
            return;

        _recorder.UseOnAudioFilterRead = IsAnyAudioPlaying;
    }

    private void OnDestroy()
    {
        foreach (var source in SoundBoard.Sounds)
            source.Stop();

        Instance = null;
    }

    /// <summary>
    ///     This is for client-side playback
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (Instance is null)
            return;

        var flen = data.Length;
        var len = flen / channels;
        float[]? tmp = null;

        foreach (var sound in SoundBoard.Sounds.Where(s => s.IsPlaying))
        {
            tmp ??= MemManager.NewArray(len);
            var vol = sound.LogVolume;
            sound.Source.Read(tmp, 48_000, false);
            for (var i = 0; i < flen; i++)
                data[i] += tmp[i / channels] * vol;
        }

        if (tmp is not null)
            MemManager.FreeArray(tmp);
    }

    /// <summary>
    ///     Initialize the sound engine.
    /// </summary>
    /// <returns>True if no other sound engine exists. False otherwise.</returns>
    public bool Init()
    {
        if (Instance is not null)
        {
            Entry.LogSource.LogError("Cannot initialize SoundManager when one already exists.");
            return false;
        }

        Instance = this;
        _recorder = gameObject.GetComponent<Recorder>();
        _recorder.Bitrate = 128_000;
        _recorder.SamplingRate = SamplingRate.Sampling48000;
        _recorder.FrameDuration = OpusCodec.FrameDuration.Frame20ms;
        _recorder.ReliableMode = true;

        Entry.LogSource.LogInfo("SoundEngine has been initialized.");
        return true;
    }
}