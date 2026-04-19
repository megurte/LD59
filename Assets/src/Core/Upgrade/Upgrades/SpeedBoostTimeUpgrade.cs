using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class SpeedBoostTimeUpgrade : UpgradeBase
    {
        public override string Name => "Engine upgrade";
        public override string Desc => "Permanently increases boost duration";
        public override Sprite Icon => null;
        
        public override void Execute()
        {
            Global.GameProgress.PlayerState.speedBoostTime += 0.3f;
        }
    }
}