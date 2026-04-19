using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class ExplosionProjectileUpgrade : IUpgrade
    {
        public string Name => "Mine shell";
        public string Desc => "Replaces projectiles by explosion mines";
        public Sprite Icon => null;
        
        public void Execute()
        {
            Global.GameProgress.PlayerState.mineProjectiles = true;
        }
    }
}