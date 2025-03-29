using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
// Remove Unity dependency if you're not targeting Unity specifically

namespace SoundBoard.Helpers;

public static class KeyHelper
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static readonly Dictionary<ConsoleKey, bool> KeyBindStates =
        Enum.GetValues(typeof(ConsoleKey)).Cast<ConsoleKey>().ToDictionary(k => k, _ => false);
    
    public static event Action<ConsoleKey, bool>? OnKeyStateChanged;
    
    public static void Poll()
    {
        foreach (var key in KeyBindStates.Keys)
        { 
            var isDown = IsKeyDown(key);
            
            if (KeyBindStates[key] == isDown) continue;
            KeyBindStates[key] = isDown;
            OnKeyStateChanged?.Invoke(key, isDown);
        }
    }
    
    private static bool IsKeyDown(ConsoleKey key) 
        => (GetAsyncKeyState((int)key) & 0x8000) != 0;
}