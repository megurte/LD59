using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class CannonCooldownUpgrade : IUpgrade
    {
        public string Name => "Old gunner";
        public string Desc => "Permanently reduces cooldown of cannon's fire";
        public Sprite Icon => null;
        
        public void Execute()
        {
            Global.GameProgress.PlayerState.cannonFireCooldownModifier -= 0.2f;
        }
    }
}