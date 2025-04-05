using Newtonsoft.Json;
using UnityEngine;

namespace SoundBoard.Models.UI;

public class SoundItemConfig
{
    [JsonProperty] public string Name { get; set; } = string.Empty;

    [JsonProperty] public KeyCode Key { get; set; } = KeyCode.None;

    [JsonProperty] public float Volume { get; set; } = 100f;

    [JsonProperty] public bool State { get; set; }
}