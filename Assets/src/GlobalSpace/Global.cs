using Common;
using Common.UI;
using Core.Submarine;

namespace GlobalSpace
{
    public static class Global
    {
        // UI
        public static Fader GlobalFader;

        public static GameProgress GameProgress;

        public static AudioController AudioController;
        public static TextController TextController;
        public static ToolController ToolController;
        
        // Core
        public static SubmarineHarpoonController HarpoonController { get; set; }
        public static SubmarineMovementController SubmarineMovement { get; set; }
        
        // Factories

        public static EffectFactory EffectFactory = new();
    }
}