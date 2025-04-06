using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Sentry;
using Sentry.Unity;
using Shared;
using UnityEngine;

namespace SoundBoard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("tgo.shared", "1.0.0.0")]
internal sealed class Entry : BaseUnityPlugin
{
    private const string PluginGuid = "tgo.soundboard";
    private const string PluginName = "SoundBoard";
    private const string PluginVersion = "1.0.2.0";
    private static readonly Harmony Harmony = new(PluginGuid);

    public static readonly Core.SoundBoard SoundBoard = new();

    private static readonly string AsmRefName = typeof(Entry).Namespace!.ToLowerInvariant();

    static Entry()
    {
        API.OnException += OnException;
    }

    internal static ManualLogSource LogSource { get; } = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
    internal static SentryUnitySdk? SentryLifetimeObject;
    
    private void Awake()
    {
        SentryLifetimeObject = SentryUnity.Init(options =>
        {
            options.Dsn = "https://c433578e608a1f775af692c1f9c76acf@devsentry.theguy920.dev/4";
            options.AutoSessionTracking = true;
            options.AttachStacktrace = false;
            options.DisableFileWrite = true;
#if !DEBUG
            options.Release = $"{PluginName}@{PluginVersion}";
            options.Environment = "production";
#elif MEGA_DEBUG
            options.Debug = true;
            options.Environment = "development";
            options.DiagnosticLevel = SentryLevel.Debug;
#endif
        });
        
        Harmony.PatchAll();
        SoundBoard.Initialize();
    }

    private static void OnException(Exception obj, LogType logType)
    {
        var message = $"{obj.Message}{obj.Source}{obj.StackTrace}";
        if (!message.ToLowerInvariant().Contains(AsmRefName)) return;
        // SentrySdk.CaptureException(obj);
    }
}