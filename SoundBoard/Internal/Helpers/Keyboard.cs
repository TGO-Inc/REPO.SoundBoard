using System.Runtime.InteropServices;
using UnityEngine;

namespace SoundBoard.Internal.Helpers;

public static class Keyboard
{
    private static readonly Dictionary<KeyCode, bool> KeyBindStates =
        Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().Distinct().ToDictionary(k => k, _ => false);
    public static event Action<KeyCode, bool>? OnKeyStateChanged;
    
    internal static void Poll()
    {
        foreach (var key in BepInEx.UnityInput.Current.SupportedKeyCodes)
        {
            var isDown = BepInEx.UnityInput.Current.GetKeyDown(key);
            if (KeyBindStates[key] == isDown) continue;
            KeyBindStates[key] = isDown;
            OnKeyStateChanged?.Invoke(key, isDown);
        }
    }
}