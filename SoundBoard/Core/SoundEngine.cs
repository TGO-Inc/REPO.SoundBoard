using System.Collections.Concurrent;
using Photon.Voice;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using SoundBoard.Models.Audio;
using SoundBoard.Models.Collections;
using SoundBoard.Models.Sources;
using UnityEngine;

namespace SoundBoard.Core;

/// <summary>
/// The sound engine only exists when PlayerVoiceChat is active.
/// </summary>
public class SoundEngine : MonoBehaviour
{
    public static SoundEngine? Instance { get; private set; }
    public readonly ConcurrentDictionary<Guid, StaticSource> StaticSources = [];
    
    private Recorder? _recorder;
    private bool _oldVoiceDetection;
    public readonly FastQueue AudioBuffer = new();
    
    /// <summary>
    /// Add a static audio source to the sound manager.
    /// </summary>
    /// <param name="staticSource"><see cref="StaticSource"/></param>
    /// <returns>True if successfully added. False otherwise</returns>
    public bool AddStaticSource(StaticSource staticSource) 
        => this.StaticSources.TryAdd(staticSource.Id, staticSource);
    
    /// <summary>
    /// Request play the sound with the given audio <see cref="Guid"/>.
    /// </summary>
    /// <param name="audioGuid"><see cref="Guid"/></param>
    /// <returns>True if request successful. False otherwise.</returns>
    public bool Play(Guid audioGuid)
    {
        if (!this.StaticSources.TryGetValue(audioGuid, out var source))
            return false;

        source.Stop();
        source.Reset();
        source.Play();
        return true;
    }
    
    /// <summary>
    /// Request toggle the sound with the given audio <see cref="Guid"/>.
    /// </summary>
    /// <param name="audioGuid"></param>
    /// <returns></returns>
    public bool Toggle(Guid audioGuid)
    {
        if (!this.StaticSources.TryGetValue(audioGuid, out var source))
            return false;

        source.Toggle();
        return true;
    }
    
    /// <summary>
    /// Request stop the sound with the given audio <see cref="Guid"/>.
    /// </summary>
    /// <param name="audioGuid"><see cref="Guid"/></param>
    /// <returns>True if request successful. False otherwise.</returns>
    public bool StopPlaying(Guid audioGuid)
    {
        if (!this.StaticSources.TryGetValue(audioGuid, out var source))
            return false;

        source.Stop();
        source.Reset();
        return true;
    }
    
    /// <summary>
    /// Initialize the sound manager.
    /// </summary>
    /// <returns>True if first initialization. False otherwise.</returns>
    public bool Init()
    {
        if (Instance is not null)
        {
            Entry.LogSource.LogWarning("Cannot initialize SoundManager when one already exists.");
            return false;
        }

        Instance = this;
        this._recorder = this.gameObject.GetComponent<Recorder>();
        this._oldVoiceDetection = this._recorder.VoiceDetection;

        this._recorder.Bitrate = 128_000;
        this._recorder.VoiceDetectionThreshold = 0;
        this._recorder.VoiceDetection = false;
        this._recorder.ReliableMode = true;
        this._recorder.SamplingRate = SamplingRate.Sampling48000;
        this._recorder.FrameDuration = OpusCodec.FrameDuration.Frame20ms;
        
        return true;
    }
    
    /// <summary>
    /// Static audio data callback.
    /// </summary>
    /// <param name="data"><see cref="float"/>[]</param>
    /// <param name="length"><see cref="int"/></param>
    internal void OnAudioFrame(float[] data, int length) => this.AudioBuffer.Add(new AudioSample(data, length));
    
    /// <summary>
    /// The "required" frame size for audio data.
    /// </summary>
    public int RequiredFrameSize { get; private set; }
    
    /// <summary>
    /// Checks all static sources for any active audio.
    /// </summary>
    public bool IsAnyAudioPlaying => this.StaticSources.Values.Any(source => source.IsPlaying);
    
    private void FixedUpdate()
    {
        if (Instance is null || this._recorder is null)
            return;
        
        if (IsAnyAudioPlaying)
        {
            if (this._recorder.VoiceDetection)
                this._recorder.VoiceDetection = false;
        }
        else
        {
            if (this._recorder.VoiceDetection != this._oldVoiceDetection)
                this._recorder.VoiceDetection = this._oldVoiceDetection;
        }
    }
    
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (Instance is null)
            return;
        
        var len = data.Length;
        
        if (!this.AudioBuffer.HasAtLeast(len))
            return;

        var holder = this.AudioBuffer.NewArray(len);
        this.AudioBuffer.Fill(holder);
        
        for (var i = 0; i < len; i++)
            data[i] = holder[i];
        
        this.AudioBuffer.FreeArray(holder);
    }

    private void OnDestroy()
    {
        foreach (var source in StaticSources)
            source.Value.Stop();
        
        Instance = null;
    }
}