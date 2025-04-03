using System.Collections.Concurrent;
using System.Reflection;
using Shared.Game;
using SoundBoard.Internal.Helpers;
using SoundBoard.Models.Audio;

namespace SoundBoard.Core;

/// <summary>
/// The Sound Board persists until the game is closed.
/// </summary>
internal sealed class SoundBoard() : EarlyMonoBehaviour(true)
{
    public UserSound[] Sounds = [];
    private readonly ConcurrentBag<UserSound> _sounds = [];
    private readonly Dictionary<ConsoleKey, List<UserSound>> _soundsByKey = [];
    private SoundEngine? _soundEngine;

    protected override void Awake()
    {
        this.PreLoadAudioFiles();
        Keyboard.OnKeyStateChanged += OnKeyStateChanged;
        Entry.LogSource.LogInfo($"Finished setup.");
    }

    internal void RegisterSoundEngine(PlayerVoiceChat instance)
    {
        this._soundEngine = instance.gameObject.AddComponent<SoundEngine>();
        this._soundEngine.Init();
    }
    
    private void PreLoadAudioFiles()
    {
        if (this._sounds.Count != 0) return;

        foreach (var path in FetchValidFiles())
        {
            var userSound = UserSound.CreateFromPath(path, OnChangeKeyBind);
            if (userSound.Source.Length == 0)
                continue;
            
            this._sounds.Add(userSound);
            Entry.LogSource.LogInfo($"Loaded \"{userSound.Name}\"");
        }
        
        this.Sounds = this._sounds.ToArray();
    }
    
    public static readonly string AudioDirectory =
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Audio");
    
    private static IEnumerable<string> FetchValidFiles()
    {
        // TODO: Implement fetching audio files.
        var directoryInfo = new DirectoryInfo(AudioDirectory);

        // Logger.LogInfo($"Searching in \"{directoryInfo.FullName}\"...");
        
        foreach (var file in directoryInfo.GetFiles())
            if (AudioFile.SupportedExtensions.ContainsKey(file.Extension.ToLowerInvariant()))
                yield return file.FullName;
    }

    public void GetNewKeyBind(Action<ConsoleKey> callback)
    {
        if (this._newKeyBind != null)
            return;
        
        this._newKeyBind = callback;
    }

    public void CancelNewKeyBind()
    {
        this._newKeyBind = null;
    }

    private Action<ConsoleKey>? _newKeyBind = null;
    private void OnKeyStateChanged(ConsoleKey key, bool state)
    {
        if (!state)
            return;
        
        if (this._newKeyBind is not null)
        {
            this._newKeyBind?.Invoke(key);
            this._newKeyBind = null;
            return;
        }

        if (!this._soundsByKey.TryGetValue(key, out var userSounds)) return;
        
        foreach (var sound in userSounds)
            sound.Signal();
    }
    
    private void OnChangeKeyBind(UserSound sound, ConsoleKey newKey)
    {
        // remove old
        foreach (var (_, soundList) in this._soundsByKey)
            if (soundList.Contains(sound))
                soundList.Remove(sound);
        
        Entry.LogSource.LogWarning("New keybind: " + newKey + " for " + sound.Name);
        // add new
        if (this._soundsByKey.TryGetValue(newKey, out var userSounds))
            userSounds.Add(sound);
        else
            this._soundsByKey.Add(newKey, [sound]);
    }
    
    protected override void FixedUpdate() => Keyboard.Poll();
}

