using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Shared;
using UnityEngine;

namespace SoundBoard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion), 
 BepInDependency("tgo.shared", "1.0.0.0")/*, 
 BepInDependency("tgo.moddedmenu", "1.0.0.0")*/]
internal sealed class Entry : BaseUnityPlugin
{
    private const string PluginGuid = "tgo.soundboard";
    private const string PluginName = "SoundBoard";
    private const string PluginVersion = "1.0.2.0";
    private static readonly Harmony Harmony = new(PluginGuid);
    internal static ManualLogSource LogSource { get; } = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
    
    public static readonly Core.SoundBoard SoundBoard = new();

    static Entry()
    {
        SentrySdk.Init(options =>
        {
            options.Dsn = "https://c433578e608a1f775af692c1f9c76acf@devsentry.theguy920.dev/4";
            options.Debug = false;
            options.AutoSessionTracking = true;
            options.IsGlobalModeEnabled = true;
            options.AttachStacktrace = false;
            options.DisableFileWrite = true;
            options.StackTraceMode = StackTraceMode.Enhanced;
            #if !DEBUG 
            options.Release = $"{PluginName}@{PluginVersion}";
            #endif
        });

        SentrySdk.ConfigureScope(scope => { scope.Level = SentryLevel.Warning; });
        API.OnException += OnException;
    }

    private static string AsmRefName = typeof(Entry).Namespace!.ToLowerInvariant();
    private static void OnException(Exception obj, LogType logType)
    {
        var message = $"{obj.Message}{obj.Source}{obj.StackTrace}";
        if (!message.ToLowerInvariant().Contains(AsmRefName)) return;
        SentrySdk.CaptureException(obj);
    }

    private void Awake()
    {
        Harmony.PatchAll();
        SoundBoard.Initialize();
    }
}