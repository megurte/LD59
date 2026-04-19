using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class HarpoonSpeedUpgrade : IUpgrade
    {
        public string Name => "Swift Harpoon";
        public string Desc => "Permanently increases harpoons speed";
        public Sprite Icon => null;
        
        public void Execute()
        {
            Global.GameProgress.PlayerState.harpoonSpeedModifier += 0.3f;
        }
    }
}