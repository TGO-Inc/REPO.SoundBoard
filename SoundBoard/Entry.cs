using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Sentry;
using Sentry.Unity;
using Shared.Core.Converters;

namespace SoundBoard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("tgo.shared", "1.1.4.0")]
internal sealed class Entry : BaseUnityPlugin
{
    private const string PluginGuid = "tgo.soundboard";
    private const string PluginName = "SoundBoard";
    private const string PluginVersion = "1.0.2.0";
    private static readonly Harmony Harmony = new(PluginGuid);

    public static readonly Core.SoundBoard SoundBoard = new();

    internal static ManualLogSource LogSource { get; } = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
    internal static SentryUnitySdk? SentryLifetimeObject;
    internal static SentrySdk? SentrySDK => SentryLifetimeObject!.SentrySdk;
    
    private void Awake()
    {
        SentryLifetimeObject = SentryUnity.Init(options =>
        {
            options.Dsn = "https://c433578e608a1f775af692c1f9c76acf@devsentry.theguy920.dev/4";
            options.DiagnosticLogger = new BepLog2SenLog(LogSource, SentryLevel.Debug);
            options.AutoSessionTracking = true;
#if GH_RELEASE
            options.Release = $"GH-{PluginName}@{PluginVersion}";
            options.Environment = "production";
#elif MEGA_DEBUG
            options.Debug = true;
            options.Environment = "development";
            options.DiagnosticLevel = SentryLevel.Debug;
#elif !DEBUG
            options.Release = $"{PluginName}@{PluginVersion}";
            options.Environment = "production";
#endif
        });
        
        Harmony.PatchAll();
        SoundBoard.Initialize();
    }
}