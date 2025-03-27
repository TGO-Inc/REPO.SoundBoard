using System.Collections.Concurrent;

namespace SoundBoard.Sound.Models;

public class RawAudio (float[] audio)
{
    private int _index = 0;
    
    /// <summary>
    /// Read-only status of the audio source.
    /// </summary>
    public bool IsPlaying { get; private set; }
    
    /// <summary>
    /// Play the audio source.
    /// </summary>
    public void Play() => IsPlaying = true;
    
    /// <summary>
    /// Stop the audio source.
    /// </summary>
    public void Stop() => IsPlaying = false;
    
    /// <summary>
    /// Reset the audio source to the beginning.
    /// </summary>
    public void Reset() => _index = 0;
    
    /// <summary>
    /// Read the next frame of audio into a new buffer.
    /// </summary>
    /// <param name="frameSize"><see cref="int"/></param>
    /// <returns></returns>
    public float[]? Read(int frameSize)
    {
        if (!IsPlaying)
            return null;

        if (_index >= audio.Length)
            return null;
        
        var buffer = new float[frameSize];
        Array.Copy(audio, _index, buffer, 0, Math.Min(frameSize, audio.Length - _index));
        _index += frameSize;
        
        return buffer;
    }
    
    /// <summary>
    /// Read the next frame of audio into the given buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    public void Read(float[] buffer)
    {
        if (!IsPlaying)
            return;

        if (_index >= audio.Length)
            return;
        
        Array.Copy(audio, _index, buffer, 0, Math.Min(buffer.Length, audio.Length - _index));
        _index += buffer.Length;
    }
    
    /// <summary>
    /// Read the next frame of audio into the given buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    /// <param name="length"><see cref="int"/></param>
    public void Read(float[] buffer, int length)
    {
        if (!IsPlaying)
            return;

        if (_index >= audio.Length)
            return;
        
        Array.Copy(audio, _index, buffer, 0, Math.Min(length, audio.Length - _index));
        _index += length;
    }
}