using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class CannonCooldownUpgrade : UpgradeBase
    {
        public override string Name => "Old gunner";
        public override string Desc => "Permanently reduces cooldown of cannon's fire";
        public override Sprite Icon => Resources.Load<Sprite>("CMS/Sprites/cannonUpgradeIcon");
        
        public override void Execute()
        {
            Global.GameProgress.PlayerState.cannonFireCooldownModifier = Mathf.Max(
                0.2f,
                Global.GameProgress.PlayerState.cannonFireCooldownModifier - 0.2f);
        }
    }
}
