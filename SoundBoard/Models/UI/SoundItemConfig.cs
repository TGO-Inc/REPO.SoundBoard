using System.Text.Json.Serialization;
using UnityEngine;

namespace SoundBoard.Models.UI;

public class SoundItemConfig
{
    [JsonInclude]
    public string Name { get; set; } = string.Empty;
    [JsonInclude]
    public KeyCode Key { get; set; } = KeyCode.None;
    [JsonInclude]
    public float Volume { get; set; } = 100f;
    [JsonInclude]
    public bool State { get; set; } = false;
    
}