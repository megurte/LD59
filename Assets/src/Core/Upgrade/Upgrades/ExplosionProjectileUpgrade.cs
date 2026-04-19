using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class ExplosionProjectileUpgrade : UpgradeBase
    {
        public override string Name => "Mine shell";
        public override string Desc => "Replaces projectiles by explosion mines";
        public override Sprite Icon => null;
        public override ICondition Condition => new MissingMineProjectilesCondition();
        
        public override void Execute()
        {
            Global.GameProgress.PlayerState.mineProjectiles = true;
        }
    }
    
    public class MissingMineProjectilesCondition : ICondition
    {
        public bool IsSatisfied() => !Global.GameProgress.PlayerState.mineProjectiles;
    }
}