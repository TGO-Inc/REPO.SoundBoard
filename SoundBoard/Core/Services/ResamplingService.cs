using NAudio.Dsp;
using SoundBoard.Models.Audio;

namespace SoundBoard.Core.Services;

public static class ResamplingService
{
    public static IAudioStream ResampleMono(IAudioStream input, int outputSampleRate)
    {
        if (input.SampleRate == outputSampleRate)
            return input;
        
        var reSampler = new WdlResampler();
        reSampler.SetMode(true, 2, false);
        reSampler.SetFilterParms();
        reSampler.SetFeedMode(false);
        reSampler.SetRates(input.SampleRate, outputSampleRate);
        
        // Calculate expected output size based on sample rate ratio
        var inputLength = (int)input.Length;
        var outputLength = (int)Math.Ceiling(inputLength * ((double)outputSampleRate / input.SampleRate));
        var outputBuffer = new float[outputLength];
        
        input.Reset();

        // Prepare for resampling
        var numToProcess = reSampler.ResamplePrepare(outputLength, 1, out var inBuffer, out var inBufferOffset);
        
        // Copy input data to resampler buffer
        input.Read(inBuffer, inBufferOffset, inputLength, false);
        
        // Perform resampling
        var samplesWritten = reSampler.ResampleOut(outputBuffer, 0, Math.Min(inputLength, numToProcess), outputLength, 1);
        return new AudioStream(outputBuffer, outputSampleRate);
    }
    
    public static void ResampleMonoAsync(IAudioStream input, int outputSampleRate, Action<IAudioStream> callback)
    {
        Task.Run(() =>
        {
            var result = ResampleMono(input, outputSampleRate);
            callback.Invoke(result);
        });
    }
}