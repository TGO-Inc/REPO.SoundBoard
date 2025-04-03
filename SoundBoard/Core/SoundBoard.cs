using System.Collections.Concurrent;
using System.Reflection;
using Shared.Game;
using SoundBoard.Core.Services;
using SoundBoard.Internal.Helpers;
using SoundBoard.Models.Audio;
using SoundBoard.Models.UI;
using UnityEngine;

namespace SoundBoard.Core;

/// <summary>
/// The Sound Board persists until the game is closed.
/// </summary>
internal sealed class SoundBoard() : EarlyMonoBehaviour(true)
{
    public UserSound[] Sounds = [];
    public readonly ConfigFileService FileService = new(Path.Combine(AudioDirectory, "audio.conf"));
    
    private readonly ConcurrentBag<UserSound> _sounds = [];
    private readonly Dictionary<KeyCode, List<UserSound>> _soundsByKey = [];
    private SoundEngine? _soundEngine;

    protected override void Awake()
    {
        FileService.Load();
        Task.Run(PreLoadAudioFiles);
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

        FileService.Load();
        
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

        foreach (var file in directoryInfo.GetFiles())
            if (AudioFile.SupportedExtensions.ContainsKey(file.Extension.ToLowerInvariant()))
                yield return file.FullName;
    }

    public void GetNewKeyBind(Action<KeyCode> callback)
    {
        if (this._newKeyBind != null)
            return;
        
        this._newKeyBind = callback;
    }

    public void CancelNewKeyBind()
    {
        this._newKeyBind = null;
    }

    private Action<KeyCode>? _newKeyBind = null;
    private void OnKeyStateChanged(KeyCode key, bool state)
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
    
    private void OnChangeKeyBind(UserSound sound, KeyCode newKey)
    {
        // remove old
        foreach (var item in 
                 this._soundsByKey.Where(item => item.Value.Contains(sound)))
            item.Value.Remove(sound);
        
        // add new
        if (this._soundsByKey.TryGetValue(newKey, out var userSounds))
            userSounds.Add(sound);
        else
            this._soundsByKey.Add(newKey, [sound]);
    }
    
    protected override void Update() => Keyboard.Poll();
}

