using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundBoard.Models.Audio;

namespace SoundBoard.Models.Sources;

/// <summary>
/// A once-initialized read-only audio source.
/// </summary>
public class StaticSource
{
    private const int Channels = 2;
    private const int SampleRate = 48_000;
    
    private int _index = 0;
    private readonly float[] _buffer;
    
    /// <summary>
    /// Create a new static audio source.
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="outputSampleRate"></param>
    /// <param name="outputChannels"></param>
    public StaticSource(AudioFile audio, int outputSampleRate = SampleRate, int outputChannels = Channels)
    {
        WaveStream? waveStream = audio.GuessedType switch
        {
            AudioFormat.MP3 => new Mp3FileReader(audio.Stream),
            AudioFormat.OGG => new VorbisWaveReader(audio.Stream),
            AudioFormat.AIFF => new AiffFileReader(audio.Stream),
            // AudioFileType.FLAC => new FlacReader(audio.Stream),
            AudioFormat.FLAC => null,
            AudioFormat.WAV or AudioFormat.PCM => new WaveFileReader(audio.Stream),
            AudioFormat.M4A or AudioFormat.WMA or AudioFormat.AAC => new StreamMediaFoundationReader(
                audio.Stream),
            AudioFormat.ALAC or AudioFormat.OPUS => null,
            AudioFormat.UNKNOWN or _ => null
        };
        
        if (waveStream is null)
        {
            _buffer = [];
            return;
        }
            
        var sampleProvider = waveStream.ToSampleProvider();

        // Resample if the native format doesn't match desired output.
        if (sampleProvider.WaveFormat.SampleRate != outputSampleRate || sampleProvider.WaveFormat.Channels != outputChannels)
            sampleProvider = new WdlResamplingSampleProvider(sampleProvider, outputSampleRate);
        
        // Read the entire audio stream into a float list.
        var samplesList = new List<float>();
        var buffer = new float[1024];
        
        int samplesRead;
        while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
            for (var i = 0; i < samplesRead; i++)
                samplesList.Add(buffer[i]);
        
        _buffer = samplesList.ToArray();
        Name = Path.GetFileName(audio.FullName);
        audio.Dispose();
    }

    /// <summary>
    /// Read-only status of the audio source.
    /// </summary>
    public bool IsPlaying { get; private set; }
    
    /// <summary>
    /// Unique audio <see cref="Guid"/>
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();
    
    /// <summary>
    /// Audio source playback volume.
    /// </summary>
    public float Volume { get; set; } = 1f;
    
    /// <summary>
    /// Arbitrary name for the audio source.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Length of the audio source.
    /// </summary>
    public int Length => _buffer.Length;
    
    /// <summary>
    /// Play the audio source.
    /// </summary>
    public void Play() => IsPlaying = true;
    
    /// <summary>
    /// Stop the audio source.
    /// </summary>
    public void Stop() => IsPlaying = false;
    
    /// <summary>
    /// Toggle (Play/Stop) the audio source.
    /// </summary>
    public void Toggle() => IsPlaying = !IsPlaying;
    
    /// <summary>
    /// Reset the audio source to the beginning.
    /// </summary>
    public void Reset() => _index = 0;
    
    /// <summary>
    /// Read the next frame of audio into the given buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    public void Read(float[] buffer) => Read(buffer, buffer.Length);
    
    /// <summary>
    /// Read the next frame of audio into the given buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    /// <param name="length"><see cref="int"/></param>
    public void Read(float[] buffer, int length)
    {
        if (!IsPlaying)
            return;

        if (_index >= _buffer.Length)
            return;
        
        Array.Copy(_buffer, _index, buffer, 0, Math.Min(length, _buffer.Length - _index));
        _index += length;
    }
}