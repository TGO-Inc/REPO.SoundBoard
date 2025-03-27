using System.Collections.Concurrent;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MenuLib;
using Photon.Voice.Unity;
using SoundBoard.Sound;
using SoundBoard.Sound.Models;
using UnityEngine;

namespace SoundBoard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion), BepInDependency("nickklmao.menulib", "2.1.1")]
internal sealed class SoundBoard : BaseUnityPlugin
{
    public static SoundBoard Instance { get; private set; } = null!;
    
    private const string PluginGuid = "tgo.soundboard";
    private const string PluginName = "SoundBoard";
    private const string PluginVersion = "1.0.0.0";
    
    private readonly Harmony _harmony = new Harmony(PluginGuid);
    
    private readonly ManualLogSource _manualLogSource = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
    private readonly List<byte[]> _customSounds = [];
    
    private void Awake()
    {
        Instance = this;
        Settings.Init(this._manualLogSource);

        MenuAPI.AddElementToLobbyMenu(parent =>
        {
            var playButton = MenuAPI.CreateREPOButton("AudioTest", TestPlayAudio, parent,
                localPosition: new Vector2(0f, 0f));
        });
        
        MenuAPI.AddElementToEscapeMenu(parent =>
        {
            var playButton = MenuAPI.CreateREPOButton("AudioTest", TestPlayAudio, parent,
                localPosition: new Vector2(0f, 0f));
        });

        this._customSounds.Add(LoadEmbeddedResource("test.mp3"));
        this._customSounds.Add(LoadEmbeddedResource("test2.mp3"));
        
        var song = LoadEmbeddedResource("test3.mp3");
        this._customSounds.Add(song);
        
        this._harmony.PatchAll();
        
        if (this._sounds.Count == 0)
        {
            foreach (var sound in  this._customSounds)
            {
                var audioSource = AudioHelper.RawAudioFromByteArray(sound, AudioFileType.MP3);
                if (audioSource is null)
                    continue;
                
                if (sound == song)
                    _targetTestRawAudio = audioSource;
                
                this._manualLogSource.LogInfo($"{PluginName} loaded SOUND");
                this._sounds.Add(audioSource);
            }
        }
        
        this._manualLogSource.LogInfo($"{PluginName} loaded");
    }

    private readonly ConcurrentBag<RawAudio> _sounds = [];
    private readonly ConcurrentDictionary<Guid, RawAudio> _soundMap = [];
    private CustomSoundManager? _soundManager;
    private Guid _targetTestSound;
    private RawAudio _targetTestRawAudio;
    
    /// <summary>
    /// Initialize the sound manager.
    /// </summary>
    /// <param name="instance"></param>
    internal void Init(PlayerVoiceChat instance)
    {
        if (this._soundManager is not null)
            this._soundManager = null;
        
        this._soundManager = instance.gameObject.AddComponent<CustomSoundManager>();

        foreach (var sound in _sounds)
        {
            var guid =  this._soundManager.AddStaticSource(sound);
            if (guid == Guid.Empty)
                continue;
            
            if (sound == _targetTestRawAudio)
                _targetTestSound = guid;
            
            _soundMap.TryAdd(guid, sound);
        }
        
        this._soundManager.Init();
    }
    
    private void TestPlayAudio()
    {
        this._soundManager!.Play(_targetTestSound);
    }
    
    private static byte[] LoadEmbeddedResource(string resourceName)
    {
        // Get the assembly where the resource is embedded.
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();
        var stream = assembly.GetManifestResourceStream(names.First(n => n.EndsWith(resourceName)));
        var data = new byte[stream!.Length];
        _ = stream.Read(data, 0, data.Length);
        return data;
    }
}
