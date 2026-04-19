using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class ProjectileRadiusUpgrade : UpgradeBase
    {
        public override string Name => "Mine casing";
        public override string Desc => "Permanently increases mine shell explosion radius";
        public override Sprite Icon => Resources.Load<Sprite>("CMS/Sprites/mineUpgrade");
        public override ICondition Condition => new HasMineProjectilesCondition();
        
        public override void Execute()
        {
            Global.GameProgress.PlayerState.projectileExplosionRadiusModifier += 0.3f;
        }
    }
    
    public class HasMineProjectilesCondition : ICondition
    {
        public bool IsSatisfied()
        {
            return Global.GameProgress.PlayerState.mineProjectiles;
        }
    }
}