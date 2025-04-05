using SoundBoard.Models.UI;
using UnityEngine;

namespace SoundBoard.Models.Audio;

/// <summary>
///     A fully self-managed sound item.
/// </summary>
public class UserSound
{
    public readonly SettingsPageSoundItem SettingsItem;
    public readonly MultiSampleSource Source;

    private bool _signalBehavior;
    private float _volume = 100f;

    private UserSound(string name, MultiSampleSource source, SoundItemConfig conf,
        Action<UserSound, KeyCode> changeKeyBind)
    {
        Source = source;
        Name = name;
        _signalBehavior = conf.State;
        Volume = conf.Volume;
        changeKeyBind(this, conf.Key);
        SettingsItem = SettingsPageSoundItem.CreateAndBind(
            conf, key => changeKeyBind(this, key), VolumeChanged, BehaviorChanged);
    }

    /// <summary>
    ///     Read-only status of the audio source.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    ///     Unique audio <see cref="Guid" />
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    ///     Audio source playback volume.
    /// </summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            LogVolume = (float)Math.Pow(_volume / 100, 3);
        }
    }

    /// <summary>
    ///     Scaled volume for audio processing.
    /// </summary>
    public float LogVolume { get; private set; } = 1;

    /// <summary>
    ///     Arbitrary name for the audio source.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Play the audio source.
    /// </summary>
    public void Play()
    {
        IsPlaying = true;
    }

    /// <summary>
    ///     Stop the audio source.
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
    }

    /// <summary>
    ///     Toggle (Play/Stop) the audio source.
    /// </summary>
    public void Toggle()
    {
        IsPlaying = !IsPlaying;
    }

    /// <summary>
    ///     Reset the audio source to the beginning.
    /// </summary>
    public void Reset()
    {
        Source.ResetAll();
    }

    private void VolumeChanged(float volume)
    {
        Volume = volume;
    }

    private void BehaviorChanged(bool behavior)
    {
        _signalBehavior = behavior;
    }

    /// <summary>
    ///     Signal the audio source to play or stop (keybind pressed).
    /// </summary>
    public void Signal()
    {
        Reset();

        if (_signalBehavior)
            Play();
        else
            Toggle();
    }

    public static UserSound CreateFromPath(string path, Action<UserSound, KeyCode> changeKeyBind)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        using var audioFile = new AudioFile(path);
        var audioSource = new MultiSampleSource(audioFile);
        return audioSource.Length == 0
            ? null!
            : new UserSound(name, audioSource,
                Entry.SoundBoard.FileService.GetOrDefault(name, n => new SoundItemConfig
                {
                    Name = n,
                    Volume = 100f,
                    Key = KeyCode.None,
                    State = true
                }), changeKeyBind);
    }
}