using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class SpeedBoostTimeUpgrade : IUpgrade
    {
        public string Name => "Engine upgrade";
        public string Desc => "Permanently increases boost duration";
        public Sprite Icon => null;
        
        public void Execute()
        {
            Global.GameProgress.PlayerState.speedBoostTime += 0.3f;
        }
    }
}