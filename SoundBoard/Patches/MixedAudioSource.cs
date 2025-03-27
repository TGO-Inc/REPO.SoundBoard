using Photon.Voice;
using UnityEngine;

namespace SoundBoard.Patches;

public class MixedAudioSource : IAudioPusher<float>
{
    private const int SAMPLE_RATE = 48000;
    private const int CHANNELS = 1;
    private const int FRAME_SIZE = 1024;

    private float[] micBuffer = new float[FRAME_SIZE];
    private float[] streamBuffer = new float[FRAME_SIZE];
    private float[] mixedBuffer = new float[FRAME_SIZE];

    private AudioClip micClip;
    private AudioClip injectedClip;
    private int micPosition;
    private int streamPosition;

    public void SetMicClip(AudioClip clip)
    {
        micClip = clip;
    }

    public void SetStreamClip(AudioClip clip)
    {
        injectedClip = clip;
    }

    public void PushAudioFrame(float[] frame, int channels)
    {
        if (micClip != null)
        {
            micClip.GetData(micBuffer, micPosition);
            micPosition = (micPosition + FRAME_SIZE) % micClip.samples;
        }

        if (injectedClip != null)
        {
            injectedClip.GetData(streamBuffer, streamPosition);
            streamPosition = (streamPosition + FRAME_SIZE) % injectedClip.samples;
        }

        // Mix mic and injected audio
        for (int i = 0; i < FRAME_SIZE; i++)
        {
            mixedBuffer[i] = micBuffer[i] + streamBuffer[i]; // Simple summing mix
        }

        // Push mixed audio to Photon
        Array.Copy(mixedBuffer, frame, FRAME_SIZE);
    }

    public void SetCallback(Action<float[]> callback, ObjectFactory<float[], int> bufferFactory)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public int SamplingRate { get; }
    public int Channels { get; }
    public string Error { get; }
}