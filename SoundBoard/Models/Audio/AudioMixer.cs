using SoundBoard.Core;

namespace SoundBoard.Models.Audio;

/// <summary>
/// Helper class for audio operations.
/// </summary>
public static class AudioMixer
{
    private static Core.SoundBoard SoundBoard => Entry.SoundBoard;

    /// <summary>
    /// Mixes audio from all active sources in <see cref="SoundEngine"/> into the given buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    /// <param name="samplingRate"><see cref="int"/></param>
    /// <param name="channels"></param>
    /// <param name="soundEngine"><see cref="SoundEngine"/></param>
    public static void MixAudio(float[] buffer, int samplingRate, int channels, SoundEngine soundEngine)
    {
        var micFrameSize = buffer.Length/channels;
        var atMicSampleRate = soundEngine.MemManager.NewArray(micFrameSize);
        
        foreach (var sound in SoundBoard.Sounds.Where(s => s.IsPlaying))
        {
            var vol = sound.LogVolume;
            var amt = sound.Source.Read(atMicSampleRate, samplingRate);
        
            if (amt == 0)
            {
                sound.Stop();
                Entry.LogSource.LogWarning("Audio stream has reached the end!");
                continue;
            }

            for (var i = 0; i < buffer.Length; i++)
                buffer[i] += atMicSampleRate[i/channels] * vol;
        }
        
        soundEngine.MemManager.FreeArray(atMicSampleRate);
    }
}
