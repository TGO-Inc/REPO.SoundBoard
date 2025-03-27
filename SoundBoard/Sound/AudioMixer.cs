using System.Collections.Concurrent;
using System.Timers;
using Photon.Voice;
using UnityEngine;

namespace SoundBoard.Sound;

public interface IAudioMixer : IAudioPusher<float>
{
    public event Action? AudioFinishedPlaying;
    
    /// <summary>
    /// Audio Source 1.
    /// </summary>
    public CustomAudioSource StaticSource { get; }
    
    /// <summary>
    /// Audio Source 2.
    /// </summary>
    public CustomAudioSource LiveSource { get; }
    
    /// <summary>
    /// Start the mixer.
    /// </summary>
    public void Start();
    
    /// <summary>
    /// Stop the mixer.
    /// </summary>
    public void Stop();
}