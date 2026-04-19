using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class AdvancedExtractionUpgrade : UpgradeBase
    {
        public override string Name => "Advanced Extraction";
        public override string Desc => "When ever fish killed by any projectile it drops fuel";
        public override Sprite Icon => Resources.Load<Sprite>("CMS/Sprites/Mine");
        public override ICondition Condition => new MissingAEProjectilesCondition();
        
        public override void Execute()
        {
            Global.GameProgress.PlayerState.fishFuelDrop = true;
        }
    }
    
    public class MissingAEProjectilesCondition : ICondition
    {
        public bool IsSatisfied() => !Global.GameProgress.PlayerState.fishFuelDrop;
    }
}