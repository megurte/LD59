using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class ProjectileRadiusUpgrade : IUpgrade
    {
        public string Name => "Engine upgrade";
        public string Desc => "Permanently increases boost duration";
        public Sprite Icon => null;
        
        public void Execute()
        {
            Global.GameProgress.PlayerState.projectileExplosionRadiusModifier += 0.3f;
        }
    }
}