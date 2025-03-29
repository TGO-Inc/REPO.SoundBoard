using SoundBoard.Models.Sources;
using SoundBoard.Models.UI;

namespace SoundBoard.Models.Audio;

public class UserSound
{
    public readonly StaticSource AudioSource;
    public readonly SettingsPageSoundItem SettingsItem;
    
    private UserSound(StaticSource audioSource, SettingsPageSoundItem settingsItem)
    {
        AudioSource = audioSource;
        SettingsItem = settingsItem;
    }
    
    public static UserSound CreateFromPath(string path)
    {
        using var audioFile = new AudioFile(path);
        var audioSource = new StaticSource(audioFile);
        var settingsItem = SettingsPageSoundItem.CreateAndBind(audioSource);
        return audioSource.Length == 0 ? null! : new UserSound(audioSource, settingsItem);
    }
}