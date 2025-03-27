using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using UnityEngine;

namespace SoundBoard.Sound;

public static class AudioHelper
{
    public static AudioClip? LoadAudioClipFromByteArray(byte[] data, AudioFileType type,
        int outputSampleRate = 48_000, int outputChannels = 1)
    {
        using var ms = new MemoryStream(data);
        ISampleProvider? sampleProvider = null;
        try
        {
            switch (type)
            {
                case AudioFileType.MP3:
                {
                    var reader = new Mp3FileReader(ms);
                    sampleProvider = reader.ToSampleProvider();
                    break;
                }
                case AudioFileType.WAV:
                case AudioFileType.PCM:
                {
                    var reader = new WaveFileReader(ms);
                    sampleProvider = reader.ToSampleProvider();
                    break;
                }
                case AudioFileType.OGG:
                {
                    // Requires NAudio.Vorbis package.
                    var reader = new VorbisWaveReader(ms);
                    sampleProvider = reader.ToSampleProvider();
                    break;
                }
                case AudioFileType.AIFF:
                {
                    var reader = new AiffFileReader(ms);
                    sampleProvider = reader.ToSampleProvider();
                    break;
                }
                case AudioFileType.FLAC:
                {
                    Debug.LogError("FLAC format not supported without additional libraries.");
                    return null;
                }
                case AudioFileType.AAC:
                case AudioFileType.ALAC:
                case AudioFileType.M4A:
                case AudioFileType.OPUS:
                case AudioFileType.WMA:
                case AudioFileType.UNKNOWN:
                default:
                {
                    Settings.Instance!.Logger.LogError("Audio file type " + type.ToString() + " is not supported.");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Settings.Instance!.Logger.LogError("Error reading audio data: " + ex.Message);
            return null;
        }

        // Resample if the native format doesn't match desired output.
        if (sampleProvider.WaveFormat.SampleRate != outputSampleRate || sampleProvider.WaveFormat.Channels != outputChannels)
            sampleProvider = new WdlResamplingSampleProvider(sampleProvider, outputSampleRate).ToMono();
        
        // Read the entire audio stream into a float list.
        var samplesList = new System.Collections.Generic.List<float>();
        var buffer = new float[1024];
        int samplesRead;
        
        while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
            for (var i = 0; i < samplesRead; i++)
                samplesList.Add(buffer[i]);

        var samples = samplesList.ToArray();
        var sampleCountPerChannel = samples.Length / outputChannels;
        var clip = AudioClip.Create("LoadedSoundBoardAudio", sampleCountPerChannel, outputChannels,
            outputSampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    
    private static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
    {
        var pcm = new byte[samplesCount * 2];
        int sampleIndex = 0,
            pcmIndex = 0;

        while (sampleIndex < samplesCount)
        {
            var outsample = (short)(samples[sampleIndex] * short.MaxValue);
            pcm[pcmIndex] = (byte)(outsample & 0xff);
            pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

            sampleIndex++;
            pcmIndex += 2;
        }

        return pcm;
    }
}
