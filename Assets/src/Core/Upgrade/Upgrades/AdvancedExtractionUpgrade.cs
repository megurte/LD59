using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class AdvancedExtractionUpgrade : UpgradeBase
    {
        public override string Name => "Advanced Extraction";
        public override string Desc => "When ever fish killed by mine projectile it drops fuel";
        public override Sprite Icon => Resources.Load<Sprite>("CMS/Sprites/extraction");
        
        public override void Execute()
        {
            Global.GameProgress.PlayerState.availableDropFromFish = true;
        }
    }
    
    public class MissingAEProjectilesCondition : ICondition
    {
        public bool IsSatisfied() => !Global.GameProgress.PlayerState.fishFuelDrop;
    }
}