namespace SoundBoard.Models.UI;

public interface ISoundItem
{
    public event Action<ConsoleKey> OnKeyBindChanged;
    public event Action<float> OnVolumeChanged;
    public event Action<bool> OnBehaviorChanged;
}