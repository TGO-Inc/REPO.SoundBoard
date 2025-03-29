using SoundBoard.Core;

namespace SoundBoard.Models.Audio;

/// <summary>
/// Helper class for audio operations.
/// </summary>
public static class AudioMixer
{
    /// <summary>
    /// Mixes audio from all active sources in <see cref="SoundEngine"/> into the given buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    /// <param name="soundEngine"><see cref="SoundEngine"/></param>
    public static void MixAudio(float[] buffer, SoundEngine soundEngine)
    {
        var dataLen = buffer.Length * 2;
        var noMicBuffer = soundEngine.AudioBuffer.NewArray(dataLen);
        var combined = soundEngine.StaticSources.Values;
        
        foreach (var audio in combined)
        {
            if (!audio.IsPlaying)
                continue;
            
            var audioBuf = soundEngine.AudioBuffer.NewArray(dataLen);
            audio.Read(audioBuf, dataLen);
            
            for (var i = 0; i < dataLen; i++)
            {
                buffer[(int)(i*0.5)] += audioBuf[i] * audio.Volume * 0.5f;
                noMicBuffer[i] += audioBuf[i] * audio.Volume;
            }
            
            soundEngine.AudioBuffer.FreeArray(audioBuf);
        }
        
        soundEngine.OnAudioFrame(noMicBuffer, dataLen);
    }
    
    /// <summary>
    /// Mixes audio from multiple sources into a single buffer.
    /// </summary>
    /// <param name="buffer"><see cref="short"/>[]</param>
    /// <param name="soundEngine"><see cref="SoundEngine"/></param>
    public static void MixAudio(short[] buffer, SoundEngine soundEngine)
    {
        var dataLen = buffer.Length;
        var noMicBuffer = soundEngine.AudioBuffer.NewArray(dataLen);
        var combined = soundEngine.StaticSources.Values;
        
        foreach (var audio in combined)
        {
            var audioBuf = soundEngine.AudioBuffer.NewArray(dataLen);
            audio.Read(audioBuf, dataLen);
            
            for (var i = 0; i < dataLen; i++)
            {
                buffer[i] += (short)(audioBuf[i] * audio.Volume * short.MaxValue);
                noMicBuffer[i] += audioBuf[i] * audio.Volume;
            }
            
            soundEngine.AudioBuffer.FreeArray(audioBuf);
        }
        
        soundEngine.OnAudioFrame(noMicBuffer, dataLen);
    }
}
