using System.Collections.Concurrent;
using System.Timers;
using Photon.Voice;

namespace SoundBoard.Sound;

public class MicMixer : IAudioMixer
{
    public event Action<float[]>? StaticFrameReady;
    public event Action? AudioFinishedPlaying;
    public CustomAudioSource StaticSource { get; }
    public CustomAudioSource LiveSource { get; }
    public int SamplingRate { get; }
    public int Channels { get; }
    public string? Error { get; } = null;
    
    private int BufferSize => (int)(SamplingRate * FrameDuration * Channels);
    private readonly System.Timers.Timer _timer;
    private const float FrameDuration = 0.02f; // 20 ms
    private Action<float[]>? _callback;
    private float[]? _audioBuffer;

    public MicMixer(CustomAudioSource staticSource, CustomAudioSource liveSource)
    {
        this.StaticSource = staticSource;
        this.LiveSource = liveSource;
        
        SamplingRate = this.StaticSource.SamplingRate;
        Channels = this.StaticSource.Channels;
        
        _timer = new System.Timers.Timer()
        {
            Interval = 20,
            AutoReset = true,
        };
        
        if (this.StaticSource.SamplingRate != this.LiveSource.SamplingRate || this.StaticSource.Channels != this.LiveSource.Channels)
        {
            Error = "Audio sources must have the same sampling rate and channels.";
            return;
        }
        
        this.StaticSource.ResetPosition();
        _timer.Elapsed += this.SendData;
    }
    
    public void SetCallback(Action<float[]> callback, ObjectFactory<float[], int> bufferFactory)
        => this._callback = callback;

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        this.StaticSource.ResetPosition();
        AudioFinishedPlaying?.Invoke();
    }
    
    private void SendData(object? sender, ElapsedEventArgs e)
    {
        if (this.StaticSource.IsAtEnd())
        {
            this.Stop();
            return;
        }

        if (this._callback is null)
            return;

        this._audioBuffer ??= new float[this.BufferSize];
        this.StaticSource.GetNextFrame(this._audioBuffer);

        var pos = 0;
        var liveFrame = this.LiveSource.GetNextFrame();
        while (liveFrame is not null)
        {
            // This will trim/lose some tiny amount of mic data
            if (liveFrame.Length + pos > BufferSize)
                liveFrame = [..liveFrame.Take(BufferSize - pos)];
            
            for (var i = 0; i < liveFrame.Length; i++)
                this._audioBuffer[i+pos] = Math.Max(liveFrame[i], this._audioBuffer[i+pos]);
            
            pos += liveFrame.Length;
            liveFrame = this.LiveSource.GetNextFrame();
        }
        
        this.StaticFrameReady?.Invoke(this._audioBuffer);
        this._callback(this._audioBuffer);
    }

    public void Dispose()
    {
        // throw new NotImplementedException();
    }
}