using System.Collections.Concurrent;
using UnityEngine;

namespace SoundBoard.Sound;

public enum AudioSourceType
{
    STATIC_SOURCE,
    LIVE_SOURCE
}

public class CustomAudioSource
{
    private AudioClip? _sourceClip;
    private int _sourceClipPosition = 0;
    private int _sourceClipLength = 0;
    private MemoryStream? _sourceStream;
    private readonly ConcurrentQueue<float[]> _liveStream = [];
    
    /// <summary>
    /// Audio source type.
    /// </summary>
    public readonly AudioSourceType SourceType;
    public int SamplingRate => _sourceClip?.frequency ?? 48000;
    public int Channels => _sourceClip?.channels ?? 1;
    public int Samples => _sourceClip?.samples ?? 0;
    
    public CustomAudioSource(AudioClip source)
    {
        _sourceClip = source;
        _sourceClipPosition = 0;
        _sourceClipLength = _sourceClip.samples * 2 * _sourceClip.channels;
        SourceType = AudioSourceType.STATIC_SOURCE;
    }

    public CustomAudioSource(MemoryStream source)
    {
        _sourceStream = source;
        SourceType = AudioSourceType.LIVE_SOURCE;
    }

    public CustomAudioSource(AudioSourceType sourceType = AudioSourceType.LIVE_SOURCE)
    {
        SourceType = sourceType;
    }
    
    /// <summary>
    /// Check if the audio stream has reached the end.
    /// </summary>
    /// <returns></returns>
    public bool IsAtEnd()
    {
        if (SourceType == AudioSourceType.STATIC_SOURCE)
            return _sourceClipPosition >= _sourceClipLength;
        
        return _liveStream.IsEmpty;
    }
    
    /// <summary>
    /// Reset the audio source position to 0.
    /// </summary>
    public void ResetPosition()
    {
        _sourceClipPosition = 0;
    }
    
    /// <summary>
    /// Thread-safe method add audio frames to live source.
    /// </summary>
    /// <param name="data">Arbitrary audio frame data.</param>
    /// <exception cref="InvalidOperationException">SourceType</exception>
    public void LiveSourceAdd(float[] data)
    {
        if (SourceType == AudioSourceType.STATIC_SOURCE)
            throw new InvalidOperationException("Cannot add data to a static source.");
        
        _liveStream.Enqueue(data);
    }
    
    /// <summary>
    /// Thread-safe method to get the next audio frame from the source.
    /// </summary>
    /// <param name="frameSize">Optional parameter for static sources</param>
    /// <returns><see cref="Nullable"/> <see cref="float"/>[] audio frame data.</returns>
    public float[]? GetNextFrame(int frameSize = 1024)
    {
        float[]? buffer;
        if (SourceType == AudioSourceType.STATIC_SOURCE)
        {
            buffer = new float[frameSize];
            _sourceClip!.GetData(buffer, _sourceClipPosition);
            _sourceClipPosition += buffer.Length;
        }
        else if (!_liveStream.TryDequeue(out buffer))
        {
            buffer = null;
        }
        
        return buffer;
    }
    
    /// <summary>
    /// Thread-safe method to get the next audio frame from the source.
    /// </summary>
    /// <param name="buffer">Audio frame buffer to be filled.</param>
    public void GetNextFrame(float[] buffer)
    {
        if (SourceType == AudioSourceType.STATIC_SOURCE)
        {
            _sourceClip!.GetData(buffer, _sourceClipPosition);
            _sourceClipPosition += buffer.Length;
        }
        else if (!_liveStream.TryDequeue(out var data))
        {
            data.CopyTo(buffer, 0);
        }
    }
}