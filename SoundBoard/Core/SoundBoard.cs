using System.Collections.Concurrent;
using System.Reflection;
using SoundBoard.Helpers;
using SoundBoard.Models.Audio;
using SoundBoard.Models.Game;

namespace SoundBoard.Core;

/// <summary>
/// The Sound Board persists until the game is closed.
/// </summary>
internal sealed class SoundBoard : IPersistantMonoBehaviour
{
    public IEnumerable<UserSound> Sounds => _sounds;
    private readonly ConcurrentBag<UserSound> _sounds = [];
    private SoundEngine? _soundEngine;

    public void Awake()
    {
        this.PreLoadAudioFiles();
        KeyHelper.OnKeyStateChanged += OnKeyStateChanged;
        Entry.LogSource.LogInfo($"Finished setup.");
    }

    internal void RegisterSoundEngine(PlayerVoiceChat instance)
    {
        this._soundEngine = instance.gameObject.AddComponent<SoundEngine>();

        foreach (var sound in _sounds)
            this._soundEngine.AddStaticSource(sound.AudioSource);
        
        this._soundEngine.Init();
    }
    
    private void PreLoadAudioFiles()
    {
        if (this._sounds.Count != 0) return;

        foreach (var path in FetchValidFiles())
        {
            var userSound = UserSound.CreateFromPath(path);
            if (userSound.AudioSource.Length == 0)
                continue;
            
            this._sounds.Add(userSound);
            Entry.LogSource.LogInfo($"Loaded \"{userSound.AudioSource.Name}\"");
        }
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
        if (_newKeyBind != null)
            return;
        
        Entry.LogSource.LogInfo($"Adding new key bind. {this}");
        _newKeyBind = callback;
    }

    public void CancelNewKeyBind()
    {
        _newKeyBind = null;
    }

    private Action<ConsoleKey>? _newKeyBind = null;
    private void OnKeyStateChanged(ConsoleKey key, bool state)
    {
        Entry.LogSource.LogInfo($"Key {key}: {(state ? "pressed" : "released")}");
        
        if (_newKeyBind == null || !state)
            return;
        
        Entry.LogSource.LogInfo($"Key {key} is now bound.");
        _newKeyBind(key);
        _newKeyBind = null;
    }
    
    private void Update()
    {
        KeyHelper.Poll();
    }
}
