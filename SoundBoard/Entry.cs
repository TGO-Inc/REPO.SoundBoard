using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Shared.Game;

namespace SoundBoard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion), 
 BepInDependency("tgo.shared", "1.0.0.0"), 
 BepInDependency("tgo.moddedmenu", "1.0.0.0")]
internal sealed class Entry : BaseUnityPlugin
{
    private const string PluginGuid = "tgo.soundboard";
    private const string PluginName = "SoundBoard";
    private const string PluginVersion = "1.0.0.0";
    private static readonly Harmony Harmony = new(PluginGuid);
    internal static ManualLogSource LogSource { get; } = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
    
    public static readonly Core.SoundBoard SoundBoard = new();
    
    private void Awake()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        
        SentrySdk.Init(options =>
        {
            // A Sentry Data Source Name (DSN) is required.
            // See https://docs.sentry.io/concepts/key-terms/dsn-explainer/
            // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
            options.Dsn = "https://c433578e608a1f775af692c1f9c76acf@devsentry.theguy920.dev/4";
            // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
            // This might be helpful, or might interfere with the normal operation of your application.
            // We enable it here for demonstration purposes when first trying Sentry.
            // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
            options.Debug = false;
            // This option is recommended. It enables Sentry's "Release Health" feature.
            options.AutoSessionTracking = true;
            // Enabling this option is recommended for client applications only. It ensures all threads use the same global scope.
            options.IsGlobalModeEnabled = false;
            // Send stack trace for events that were not created from an exception
            // e.g: CaptureMessage, log.LogDebug, log.LogInformation ...
            options.AttachStacktrace = true;
            // Disable file write to avoid writing files to the disk.
            options.DisableFileWrite = true;
            // Set StackTraceMode to Enhanced to get more information about the stack trace.
            options.StackTraceMode = StackTraceMode.Enhanced;
            // This option tells Sentry to capture 100% of traces. You still need to start transactions and spans.
            options.TracesSampleRate = 1.0;
        });
        
        SentrySdk.ConfigureScope(scope =>
        {
            scope.Level = SentryLevel.Warning;
        });
        
        Harmony.PatchAll();
        SoundBoard.Initialize();
    }

    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception ?? new Exception("Exception object is not Exception?");
        SentrySdk.CaptureException(ex);
    }
}