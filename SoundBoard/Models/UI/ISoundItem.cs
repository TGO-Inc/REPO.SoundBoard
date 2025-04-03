using UnityEngine;

namespace SoundBoard.Models.UI;

public interface ISoundItem
{
    public event Action<KeyCode> OnKeyBindChanged;
    public event Action<float> OnVolumeChanged;
    public event Action<bool> OnBehaviorChanged;
}