using GlobalSpace;

namespace Core.Upgrade.Upgrades
{
    public class AdvancedExtractionUpgrade : UpgradeBase
    {
        public override string Name => "Advanced Extraction";
        public override string Desc => "When ever fish killed by any projectile it drops fuel";
        public override void Execute()
        {
            Global.GameProgress.PlayerState.fishFuelDrop = true;
        }
    }
}