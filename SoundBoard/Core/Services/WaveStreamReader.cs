using NAudio.Wave;
using SoundBoard.Models.Audio;

namespace SoundBoard.Core.Services;

public class WaveStreamReader(WaveStream waveStream)
{
    public AudioStream ReadAudioStream()
    {
        var sampleProvider = waveStream.ToSampleProvider().ToMono();
        // Determine the length of the audio in samples
        var sampleCount = (int)(waveStream.Length / (waveStream.WaveFormat.BitsPerSample / 8) / waveStream.WaveFormat.Channels);
        var ret = new float[sampleCount];
        
        // Read all audio data into the buffer
        var samplesRead = sampleProvider.Read(ret, 0, sampleCount);
        
        // Resize if we didn't read as many samples as expected
        if (samplesRead < sampleCount)
            Array.Resize(ref ret, samplesRead);
        
        return new AudioStream(ret, waveStream.WaveFormat.SampleRate);
    }
}