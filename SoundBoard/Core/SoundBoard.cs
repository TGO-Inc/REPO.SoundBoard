using System.Collections.Concurrent;
using System.Reflection;
using Shared.Game;
using SoundBoard.Core.Services;
using SoundBoard.Internal.Helpers;
using SoundBoard.Models.Audio;
using UnityEngine;

namespace SoundBoard.Core;

/// <summary>
///     The Sound Board persists until the game is closed.
/// </summary>
internal sealed class SoundBoard() : EarlyMonoBehaviour(true)
{
    public static readonly string AudioDirectory =
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Audio");

    private readonly ConcurrentBag<UserSound> _sounds = [];
    private readonly Dictionary<KeyCode, List<UserSound>> _soundsByKey = [];
    public readonly ConfigFileService FileService = new(Path.Combine(AudioDirectory, "audio.conf"));

    private Action<KeyCode>? _newKeyBind;
    private SoundEngine? _soundEngine;
    public UserSound[] Sounds = [];

    protected override void Awake()
    {
        FileService.Load();
        Task.Run(PreLoadAudioFiles);
        Keyboard.OnKeyStateChanged += OnKeyStateChanged;
        Entry.LogSource.LogInfo("Finished setup.");
    }

    internal void RegisterSoundEngine(PlayerVoiceChat instance)
    {
        _soundEngine = instance.gameObject.AddComponent<SoundEngine>();
        _soundEngine.Init();
    }

    private void PreLoadAudioFiles()
    {
        if (_sounds.Count != 0) return;

        FileService.Load();

        foreach (var path in FetchValidFiles())
        {
            var userSound = UserSound.CreateFromPath(path, OnChangeKeyBind);
            if (userSound.Source.Length == 0)
                continue;

            _sounds.Add(userSound);
            Entry.LogSource.LogInfo($"Loaded \"{userSound.Name}\"");
        }

        Sounds = _sounds.ToArray();
    }

    private static IEnumerable<string> FetchValidFiles()
    {
        var directoryInfo = new DirectoryInfo(AudioDirectory);

        foreach (var file in directoryInfo.GetFiles())
            if (AudioFile.SupportedExtensions.ContainsKey(file.Extension.ToLowerInvariant()))
                yield return file.FullName;
    }

    public void GetNewKeyBind(Action<KeyCode> callback)
    {
        if (_newKeyBind != null)
            return;

        _newKeyBind = callback;
    }

    public void CancelNewKeyBind()
    {
        _newKeyBind = null;
    }

    private void OnKeyStateChanged(KeyCode key, bool state)
    {
        if (!state)
            return;

        if (_newKeyBind is not null)
        {
            _newKeyBind?.Invoke(key);
            _newKeyBind = null;
            return;
        }

        if (!_soundsByKey.TryGetValue(key, out var userSounds)) return;

        foreach (var sound in userSounds)
            sound.Signal();
    }

    private void OnChangeKeyBind(UserSound sound, KeyCode newKey)
    {
        // remove old
        foreach (var item in
                 _soundsByKey.Where(item => item.Value.Contains(sound)))
            item.Value.Remove(sound);

        // add new
        if (_soundsByKey.TryGetValue(newKey, out var userSounds))
            userSounds.Add(sound);
        else
            _soundsByKey.Add(newKey, [sound]);
    }

    protected override void Update()
    {
        Keyboard.Poll();
    }
}