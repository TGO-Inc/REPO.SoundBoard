using System.Collections.Concurrent;
using NAudio.Vorbis;
using NAudio.Wave;
using SoundBoard.Core.Services;

namespace SoundBoard.Models.Audio;

/// <summary>
/// A once-initialized read-only multisample rate audio source.
/// </summary>
public class MultiSampleSource
{
    private readonly IAudioStream _original;
    private readonly ConcurrentDictionary<int, IAudioStream?> _resampledBuffers = [];
    
    /// <summary>
    /// Create a new static audio source.
    /// </summary>
    /// <param name="audio"></param>
    public MultiSampleSource(AudioFile audio)
    {
        using WaveStream? waveStream = audio.GuessedType switch
        {
            AudioFormat.MP3 => new Mp3FileReader(audio.Stream),
            AudioFormat.OGG => new VorbisWaveReader(audio.Stream),
            AudioFormat.AIFF => new AiffFileReader(audio.Stream),
            // AudioFileType.FLAC => new FlacReader(audio.Stream),
            AudioFormat.WAV or AudioFormat.PCM => new WaveFileReader(audio.Stream),
            AudioFormat.M4A or AudioFormat.WMA or AudioFormat.AAC => new StreamMediaFoundationReader(audio.Stream),
            AudioFormat.ALAC or AudioFormat.OPUS => null,
            AudioFormat.UNKNOWN or _ => null
        };
        
        if (waveStream is null)
        {
            Entry.LogSource.LogWarning("Audio stream is null for file: " + audio.FullName);
            _original = null!;
            return;
        }

        var reader = new WaveStreamReader(waveStream);
        _original = reader.ReadAudioStream();
    }

    /// <summary>
    /// Get the original audio stream length.
    /// </summary>
    public long Length => _original.Length;

    /// <summary>
    /// Reset the audio source to the beginning.
    /// </summary>
    public void ResetAll()
    {
        foreach (var (_, audio) in _resampledBuffers)
            audio?.Reset();
    }

    /// <summary>
    /// Read the next frame of audio into the given buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    /// <param name="sampleRate"><see cref="int"/></param>
    /// <param name="advance"><see cref="bool"/></param>
    public int Read(float[] buffer, int sampleRate, bool advance = true)
    {
        if (_resampledBuffers.TryGetValue(sampleRate, out var aBuffer))
            return aBuffer?.EndOfStream != true ? aBuffer?.Read(buffer, buffer.Length, advance) ?? -1 : 0;
            
        _resampledBuffers.TryAdd(sampleRate, null);
        ResamplingService.ResampleMonoAsync(
            _original,
            sampleRate,
            newBuff => _resampledBuffers[sampleRate] = newBuff);
        
        return -1;

    }
}