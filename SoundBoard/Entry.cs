using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SoundBoard.Models.Game;
using UnityEngine;

namespace SoundBoard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion), BepInDependency("nickklmao.menulib", "2.1.1")]
internal sealed class Entry : BaseUnityPlugin
{
    private const string PluginGuid = "tgo.soundboard";
    private const string PluginName = "SoundBoard";
    private const string PluginVersion = "1.0.0.0";
    
    private static readonly Harmony Harmony = new(PluginGuid);
    internal static ManualLogSource LogSource { get; } = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
    
    public static readonly Core.SoundBoard SoundBoard = new();
    private static readonly PersistantMonoBehaviourProxy Proxy = new(SoundBoard);
    
    private void Awake()
    {
        Harmony.PatchAll();
        Proxy.Begin();
    }
}