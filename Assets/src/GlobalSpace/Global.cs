using Common;
using Common.UI;
using Core.Submarine;
using Core.Upgrade;

namespace GlobalSpace
{
    public static class Global
    {
        static Global()
        {
            Initialize();
        }

        // UI
        public static Fader GlobalFader;

        public static GameProgress GameProgress;

        public static AudioController AudioController;
        public static TextController TextController;
        public static ToolController ToolController;
        public static UpgradeSelectorController UpgradeSelectorController;
        public static UpgradeDropService UpgradeDropService;
        public static bool IsUpgradeSelectorOpen { get; set; }

        // Core
        public static SubmarineHarpoonController HarpoonController { get; set; }
        public static SubmarineMovementController SubmarineMovement { get; set; }

        // Factories
        public static EffectFactory EffectFactory = new();

        public static void Initialize()
        {
            GameProgress = new GameProgress();
            UpgradeDropService = new UpgradeDropService();
            EffectFactory = new EffectFactory();
            IsUpgradeSelectorOpen = false;
        }
    }
}
