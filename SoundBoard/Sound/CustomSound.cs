using System.Collections.Concurrent;
using Photon.Voice;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using SoundBoard.Patches;
using UnityEngine;

namespace SoundBoard.Sound;

public class CustomSound : MonoBehaviour
{
    private AudioClip? _audioClip;
    private Recorder? _recorder;
    private MicMixer? _audioMixer;

    private CustomAudioSource StaticSource;
    private CustomAudioSource LiveSource;
    
    // private MicrophoneCapture? _microphoneRedirect;
    private bool? _oldVoiceDetection = null;
    private float? _oldThreshold= null;
    private DeviceInfo? _oldDeviceInfo= null;
    private SamplingRate? _oldSamplingRate= null;

    private readonly ConcurrentQueue<float[]> _audioBuffer = [];
    private readonly ConcurrentQueue<float[]> _lateAudioBuffer = [];

    private readonly ConcurrentQueue<StatusRequest> _statusRequests = [];

    /// <summary>
    /// Mix the sound into the mic buffer and play for host.
    /// </summary>
    public void Play()
    {
        _statusRequests.Enqueue(StatusRequest.STOP);
        Task.Delay(100).ContinueWith(_ => _statusRequests.Enqueue(StatusRequest.START));
    }
    
    /// <summary>
    /// Stop the sound from playing.
    /// </summary>
    public void Stop()
    {
        this.AudioFinishedPlaying();
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    /// <summary>
    /// Load the sound from a byte array.
    /// </summary>
    /// <param name="soundData"></param>
    /// <param name="type"></param>
    public void Load(byte[] soundData, AudioFileType type)
        => Load(AudioHelper.LoadAudioClipFromByteArray(soundData, type));
    
    /// <summary>
    /// Load the sound from an AudioClip.
    /// </summary>
    /// <param name="clip"></param>
    public void Load(AudioClip? clip)
    {
        if (clip is null)
            return;
        
        _audioClip = clip;
        _recorder = this.gameObject.GetComponent<Recorder>() ?? this.gameObject.AddComponent<Recorder>();
        // _recorder.
    }

    /// <summary>
    /// Safe Load method that will only load if there is no current sound.
    /// </summary>
    /// <param name="audioData"></param>
    /// <param name="type"></param>
    public void TryInit(byte[] audioData, AudioFileType type)
    {
        if (_audioClip is null)
            Load(audioData, type);
    }
    
    private void OnAudioFrame(float[] data) =>
        _audioBuffer.Enqueue(ConvertMonoToStereo(data));
    
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_lateAudioBuffer.Any() && !_audioBuffer.Any())
            return;
        
        var dataLen = data.Length;
        
        List<float> combined = [];
        
        while (_lateAudioBuffer.TryDequeue(out var buff))
            combined.AddRange(buff);
        
        while (_audioBuffer.TryDequeue(out var buff))
            combined.AddRange(buff);
        
        if (combined.Count < dataLen)
        {
            _lateAudioBuffer.Enqueue([..combined]);
            return;
        }
        
        float[] combinedArr = [..combined];
        combinedArr[..dataLen].CopyTo(data, 0);
        
        if (combinedArr.Length >= dataLen)
            _lateAudioBuffer.Enqueue(combinedArr[dataLen..]);
    }

    private void Update()
    {
        if (!_statusRequests.TryDequeue(out var statusRequest)) return;
        switch (statusRequest)
        {
            case StatusRequest.STOP:
                this.CallStop();
                break;
            case StatusRequest.START:
                this.CallStart();
                break;
            case StatusRequest.MICROPHONE_READY:
                this.CallMicrophoneReady();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CallMicrophoneReady()
    {
        // _microphoneRedirect ??= new MicrophoneCapture(_oldDeviceInfo.Value.Name, 48_000, 1);
        // _microphoneRedirect.OnProcessedAudio += this.LiveSource.LiveSourceAdd;
        // _microphoneRedirect.StartRecording();
    }

    private void CallStop()
    {
        // if (_microphoneRedirect is not null)
        // {
        //     _microphoneRedirect.StopRecording();
        //     _microphoneRedirect.OnProcessedAudio -= this.LiveSource.LiveSourceAdd;
        // }

        if (_audioMixer is not null)
        {
            _audioMixer.Stop();
            _audioMixer.AudioFinishedPlaying -= this.AudioFinishedPlaying;
            _audioMixer.StaticFrameReady -= this.OnAudioFrame;
        }

        if (_recorder is not null)
        {
            _recorder.SourceType = Recorder.InputSourceType.Microphone;
            if (_oldVoiceDetection is not null)
                _recorder.VoiceDetection = _oldVoiceDetection.Value;
            if (_oldThreshold is not null)
                _recorder.VoiceDetectionThreshold = _oldThreshold.Value;
            if (_oldDeviceInfo is not null)
                _recorder.MicrophoneDevice = _oldDeviceInfo.Value;
            if (_oldSamplingRate is not null)
                _recorder.SamplingRate = _oldSamplingRate.Value;

            _oldVoiceDetection = null;
            _oldThreshold = null;
            _oldDeviceInfo = null;
            _oldSamplingRate = null;
        }

        MicrophonePatch.BlockMicrophone = false;
    }

    private void CallStart()
    {
        this.StaticSource ??= new CustomAudioSource(this._audioClip!);
        this.LiveSource ??= new CustomAudioSource();
        
        MicrophonePatch.BlockMicrophone = true;
        
        _recorder ??= this.GetComponent<Recorder>();
        RecorderPatch.LocalVoiceAudio.TryGetValue(_recorder, out var localVoiceAudio);
        Settings.Instance.Logger.LogInfo("LocalVoiceAudio: " + localVoiceAudio);
        
        if (localVoiceAudio is LocalVoiceAudioFloat lvAudio)
        {
            customPreprocessor = new MyCustomPreprocessor();
            customPreprocessor.OnAudioFrameProcessed += frame =>
            {
                Debug.Log($"Received audio frame with {frame.Length} samples.");
                // Process the frame as needed.
            };

            // Add the custom preprocessor to the local voice.
            lvAudio.AddPreProcessor(customPreprocessor);
            Settings.Instance.Logger.LogInfo("Preprocessor successfully added to LocalVoice.");
        }
        // var source = this.GetComponent<AudioSource>();
        // source.Stop();
        // source.clip = null;
        
        _audioMixer ??= new MicMixer(this.StaticSource, this.LiveSource);
        _audioMixer.Stop();

        _oldThreshold = _recorder.VoiceDetectionThreshold;
        _oldVoiceDetection = _recorder.VoiceDetection;
        _oldDeviceInfo = _recorder.MicrophoneDevice;
        _oldSamplingRate = _recorder.SamplingRate;

        _recorder.MicrophoneDevice = new DeviceInfo("");
        _recorder.SourceType = Recorder.InputSourceType.Factory;
        _recorder.RecordingEnabled = true;
        _recorder.TransmitEnabled = true;
        _recorder.ReliableMode = true;
        _recorder.VoiceDetectionThreshold = 0;
        _recorder.VoiceDetection = false;
        _recorder.FrameDuration = OpusCodec.FrameDuration.Frame20ms;
        _recorder.Bitrate = 64_000;
        _recorder.SamplingRate = SamplingRate.Sampling48000;
        _recorder.InputFactory = () => _audioMixer;
        
        _audioMixer.Start();
        _audioMixer.AudioFinishedPlaying += this.AudioFinishedPlaying;
        _audioMixer.StaticFrameReady += this.OnAudioFrame;
        
        Microphone.End(_oldDeviceInfo.Value.Name);
        
        Task.Delay(100).ContinueWith(_ =>
        {
            if (!_statusRequests.Any())
                _statusRequests.Enqueue(StatusRequest.MICROPHONE_READY);
        });
    }

    private void AudioFinishedPlaying()
    {
        _statusRequests.Enqueue(StatusRequest.STOP);
    }
    
    private static float[] ConvertMonoToStereo(float[] monoSamples)
    {
        // The stereo array will have twice as many samples (L, R, L, R, ...)
        var stereoSamples = new float[monoSamples.Length * 2];

        for (var i = 0; i < monoSamples.Length; i++)
        {
            // Assign the mono sample to the left and right channels
            stereoSamples[i * 2] = monoSamples[i];       // Left channel
            stereoSamples[i * 2 + 1] = monoSamples[i];     // Right channel
        }

        return stereoSamples;
    }
}

internal enum StatusRequest
{
    START,
    MICROPHONE_READY,
    STOP
}