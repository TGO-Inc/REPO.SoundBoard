using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MenuLib;
using SoundBoard.Patches;
using SoundBoard.Sound;
using UnityEngine;

namespace SoundBoard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion), BepInDependency("nickklmao.menulib", "2.1.1")]
internal sealed class SoundBoard : BaseUnityPlugin
{
    private const string PluginGuid = "tgo.soundboard";
    private const string PluginName = "SoundBoard";
    private const string PluginVersion = "1.0.0.0";
    
    private readonly Harmony _harmony = new Harmony(PluginGuid);
    
    private static readonly ManualLogSource ManualLogSource = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
    public static readonly List<byte[]> CustomSounds = [];
    
    private void Awake()
    {
        // Initialize global objects
        Settings.Init(ManualLogSource);

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

        var mp3AudioStream = LoadEmbeddedResource("test.mp3");
        CustomSounds.Add(mp3AudioStream);
        
        _harmony.PatchAll();
        ManualLogSource.LogInfo($"{PluginName} loaded");
    }
    
    private void TestPlayAudio()
    {
        ManualLogSource.LogInfo($"{PluginName} CLICK");
        
        var customSound = PlayerVoiceChat.instance.GetComponent<CustomSound>() ?? PlayerVoiceChat.instance.gameObject.AddComponent<CustomSound>();
        customSound.TryInit(CustomSounds.First(), AudioFileType.MP3);
        customSound.Play();
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
