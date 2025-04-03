using System.Runtime.InteropServices;

namespace SoundBoard.Internal.Helpers;

public static class Keyboard
{
    [DllImport("user32.dll")]
    private static extern short GetKeyState(int vKey);

    private static readonly Dictionary<ConsoleKey, bool> KeyBindStates =
        Enum.GetValues(typeof(ConsoleKey)).Cast<ConsoleKey>().ToDictionary(k => k, _ => false);
    
    private static readonly ConsoleKey[] ValidKeys = KeyBindStates.Keys.ToArray();
    
    public static event Action<ConsoleKey, bool>? OnKeyStateChanged;
    
    internal static void Poll()
    {
        foreach (var key in ValidKeys)
        { 
            var isDown = IsKeyDown(key);
            // if (key == ConsoleKey.J)
            //     Entry.LogSource.LogWarning("J key: " + isDown);

            if (KeyBindStates[key] == isDown) continue;
            KeyBindStates[key] = isDown;
            OnKeyStateChanged?.Invoke(key, isDown);
        }
    }
    
    private static bool IsKeyDown(ConsoleKey key) 
        => (GetKeyState((int)key) & 0x8000) != 0;
}