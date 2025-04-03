namespace SoundBoard.Models.UI;

public record SoundItemInit
{
    public string Name { get; set; } = string.Empty;
    public ConsoleKey Key { get; set; } = ConsoleKey.NoName;
    public bool State { get; set; } = false;
    public float Volume { get; set; } = 100f;
    
    public Action<ConsoleKey>? OnKeyBindChanged { get; set; }
    public Action<float>? OnVolumeChanged { get; set; }
    public Action<bool>? OnBehaviorChanged { get; set; }
    
}