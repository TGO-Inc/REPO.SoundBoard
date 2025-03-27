using BepInEx.Logging;

namespace SoundBoard
{
    public class Settings
    {
        public static Settings? Instance { get; private set; }
        public ManualLogSource Logger { get; private set; }

        private Settings(ManualLogSource logger)
        {
            Logger = logger;
        }

        public static void Init(ManualLogSource logger)
        {
            Instance ??= new Settings(logger);
        }
    }
}