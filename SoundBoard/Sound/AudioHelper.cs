using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundBoard.Sound.Models;
using UnityEngine;

namespace SoundBoard.Sound;

/// <summary>
/// Helper class for audio operations.
/// </summary>
internal static class AudioHelper
{
    /// <summary>
    /// Mixes audio from multiple sources into a single buffer.
    /// </summary>
    /// <param name="buffer"><see cref="float"/>[]</param>
    /// <param name="soundManager"><see cref="CustomSoundManager"/></param>
    public static void MixAudio(float[] buffer, CustomSoundManager soundManager)
    {
        var dataLen = buffer.Length;
        var noMicBuffer = soundManager.AudioBuffer.NewArray(dataLen);
        var combined = soundManager.StaticSources.Values;
        
        foreach (var audio in combined)
        {
            var audioBuf = soundManager.AudioBuffer.NewArray(dataLen);
            audio.Read(audioBuf, dataLen);
            
            for (var i = 0; i < dataLen; i++)
            {
                buffer[i] += audioBuf[i];
                noMicBuffer[i] += audioBuf[i];
            }
            
            soundManager.AudioBuffer.FreeArray(audioBuf);
        }
        
        soundManager.OnAudioFrame(noMicBuffer, dataLen);
    }
    
    /// <summary>
    /// Mixes audio from multiple sources into a single buffer.
    /// </summary>
    /// <param name="buffer"><see cref="short"/>[]</param>
    /// <param name="soundManager"><see cref="CustomSoundManager"/></param>
    public static void MixAudio(short[] buffer, CustomSoundManager soundManager)
    {
        var dataLen = buffer.Length;
        var noMicBuffer = soundManager.AudioBuffer.NewArray(dataLen);
        var combined = soundManager.StaticSources.Values;
        
        foreach (var audio in combined)
        {
            var audioBuf = soundManager.AudioBuffer.NewArray(dataLen);
            audio.Read(audioBuf, dataLen);
            
            for (var i = 0; i < dataLen; i++)
            {
                buffer[i] += (short)(audioBuf[i] * short.MaxValue);
                noMicBuffer[i] += audioBuf[i];
            }
            
            soundManager.AudioBuffer.FreeArray(audioBuf);
        }
        
        soundManager.OnAudioFrame(noMicBuffer, dataLen);
    }

    /// <summary>
    /// Converts a byte array to a RawAudio object.
    /// </summary>
    /// <param name="data"><see cref="byte"/>[]</param>
    /// <param name="type"><see cref="AudioFileType"/></param>
    /// <param name="outputSampleRate"><see cref="int"/></param>
    /// <param name="outputChannels"><see cref="int"/></param>
    /// <returns></returns>
    public static RawAudio? RawAudioFromByteArray(byte[] data, AudioFileType type,
        int outputSampleRate = 48_000, int outputChannels = 1)
    {
        using var ms = new MemoryStream(data);
        return RawAudioFromByteArray(ms, type, outputSampleRate, outputChannels);
    }
    
    /// <summary>
    /// Converts a raw audio stream to a RawAudio object.
    /// </summary>
    /// <param name="ms"><see cref="MemoryStream"/></param>
    /// <param name="type"><see cref="AudioFileType"/></param>
    /// <param name="outputSampleRate"><see cref="int"/></param>
    /// <param name="outputChannels"><see cref="int"/></param>
    /// <returns></returns>
    public static RawAudio? RawAudioFromByteArray(MemoryStream ms, AudioFileType type,
        int outputSampleRate = 48_000, int outputChannels = 1)
    {
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
            sampleProvider = new WdlResamplingSampleProvider(sampleProvider, outputSampleRate);
        
        // Read the entire audio stream into a float list.
        var samplesList = new System.Collections.Generic.List<float>();
        var buffer = new float[1024];
        int samplesRead;
        
        while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
            for (var i = 0; i < samplesRead; i++)
                samplesList.Add(buffer[i]);
        
        return new RawAudio(samplesList.ToArray());
    }
}
