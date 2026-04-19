using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class HarpoonSpeedUpgrade : UpgradeBase
    {
        public override string Name => "Swift Harpoon";
        public override string Desc => "Permanently increases harpoons speed";
        public override Sprite Icon => Resources.Load<Sprite>("CMS/Sprites/HarpoonUpgrade");
        
        public override void Execute()
        {
            Global.GameProgress.PlayerState.harpoonSpeedModifier += 0.3f;
        }
    }
}
