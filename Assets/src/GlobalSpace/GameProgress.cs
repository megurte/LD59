namespace GlobalSpace
{
    public class PlayerState
    {
        public float harpoonSpeedModifier = 1f;
        public float speedBoostTime = 1.5f;
        public float cannonFireCooldownModifier = 1f;
        public float projectileExplosionRadiusModifier = 1f;
        
        public bool availableDropFromFish = false;
        public bool mineProjectiles = false;
        public bool fishFuelDrop = false;
    }
    
    public class GameProgress
    {
        public bool skipIntro = false;
        public bool tutorialPassed = false;
        public PlayerState PlayerState { get; set; }

        public GameProgress()
        {
            PlayerState = new PlayerState();
        }
    }
}