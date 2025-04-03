using Photon.Voice;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using SoundBoard.Models.Audio;
using SoundBoard.Models.Collections;
using UnityEngine;

namespace SoundBoard.Core;

/// <summary>
/// The sound engine only exists when PlayerVoiceChat is active.
/// </summary>
public class SoundEngine : MonoBehaviour
{
    public static SoundEngine? Instance { get; private set; }
    public readonly FastMemory MemManager = new();
    
    private Recorder? _recorder;
    private static SoundBoard SoundBoard => Entry.SoundBoard;

    /// <summary>
    /// Initialize the sound engine.
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
        this._recorder = this.gameObject.GetComponent<Recorder>();
        this._recorder.Bitrate = 128_000;
        this._recorder.SamplingRate = SamplingRate.Sampling48000;
        this._recorder.FrameDuration = OpusCodec.FrameDuration.Frame20ms;
        this._recorder.ReliableMode = true;
        
        Entry.LogSource.LogWarning("SoundEngine has been initialized.");
        return true;
    }

    private void FixedUpdate()
    {
        if (Instance is null || this._recorder is null)
            return;

        this._recorder.UseOnAudioFilterRead = IsAnyAudioPlaying;
    }
    
    // /// <summary>
    // /// Static audio data callback.
    // /// </summary>
    // /// <param name="data"><see cref="float"/>[]</param>
    // internal void OnAudioFrame(float[] data) 
    //     => this._audioBuffer.Add(new AudioSample(data, data.Length));
    
    /// <summary>
    /// Checks all static sources for any active audio.
    /// </summary>
    public static bool IsAnyAudioPlaying => SoundBoard.Sounds.Any(sound => sound.IsPlaying);

    /// <summary>
    /// The required frame size for client audio playback.
    /// </summary>
    public long RequiredFrameSize { get; private set; } = 1024;
    
    /// <summary>
    /// This is for client-side playback
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (Instance is null)
            return;
        
        var len = data.Length / channels;
        if (this.RequiredFrameSize != len)
            this.RequiredFrameSize = len;
        
        var tmp = this.MemManager.NewArray(len);

        foreach (var sound in SoundBoard.Sounds.Where(s => s.IsPlaying))
        {
            var vol = sound.LogVolume;
            sound.Source.Read(tmp, 48_000, false);
            for (var i = 0; i < data.Length; i++)
                data[i] += tmp[i / channels] * vol;
        }
        
        this.MemManager.FreeArray(tmp);
    }

    private void OnDestroy()
    {
        foreach (var source in SoundBoard.Sounds)
            source.Stop();
        
        Instance = null;
    }
}