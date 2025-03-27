using System.Collections.Concurrent;
using Photon.Voice;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using SoundBoard.Sound.Models;
using UnityEngine;

namespace SoundBoard.Sound;

public class CustomSoundManager : MonoBehaviour
{
    public static CustomSoundManager? Instance { get; private set; }
    public readonly ConcurrentDictionary<Guid, RawAudio> StaticSources = [];
    
    private Recorder? _recorder;
    private bool? _oldVoiceDetection = null;
    private float? _oldThreshold= null;
    internal readonly FastQueue AudioBuffer = new();
    
    /// <summary>
    /// Add a static audio source to the sound manager.
    /// </summary>
    /// <param name="staticSource"><see cref="RawAudio"/></param>
    /// <returns>The <see cref="Guid"/> assigned to the audio.</returns>
    public Guid AddStaticSource(RawAudio staticSource)
    {
        var guid = Guid.NewGuid();
        return this.StaticSources.TryAdd(guid, staticSource) ? guid : Guid.Empty;
    }
    
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
            return false;

        Instance = this;
        this._recorder ??= this.gameObject.GetComponent<Recorder>();
        
        return true;
    }
    
    /// <summary>
    /// Static audio data callback.
    /// </summary>
    /// <param name="data"><see cref="float"/>[]</param>
    /// <param name="length"><see cref="int"/></param>
    internal void OnAudioFrame(float[] data, int length) => this.AudioBuffer.Add(new Sample(data, length));
    
    /// <summary>
    /// The "required" frame size for audio data.
    /// </summary>
    public int RequiredFrameSize { get; private set; }
    
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (Instance is null)
            return;
        
        if (RequiredFrameSize != data.Length)
            RequiredFrameSize = data.Length;
        
        if (!this.AudioBuffer.HasAtLeast(data.Length))
        {
            Settings.Instance.Logger.LogWarning("NO AUDIO DATA READY!!");
            return;
        }

        this.AudioBuffer.Fill(data);
    }
    
    private void FixedUpdate()
    {
        if (!this.StaticSources.Values.Any(s => s.IsPlaying))
        {
            if (this._oldVoiceDetection is not null)
            {
                this._recorder.VoiceDetection = this._oldVoiceDetection.Value;
                this._oldVoiceDetection = null;
            }
            
            if (this._oldThreshold is not null)
            {
                this._recorder.VoiceDetectionThreshold = this._oldThreshold.Value;
                this._oldThreshold = null;
            }
        }
        else
        {
            if (this._oldVoiceDetection is null)
            {
                this._oldVoiceDetection = this._recorder.VoiceDetection;
                this._recorder.VoiceDetection = false;
            }
            
            if (this._oldThreshold is null)
            {
                this._oldThreshold = this._recorder.VoiceDetectionThreshold;
                this._recorder.VoiceDetectionThreshold = 0;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var source in StaticSources)
            source.Value.Stop();
        
        Instance = null;
    }
}