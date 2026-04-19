using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class SubmarineSpeedBoostUpgrade : IUpgrade
    {
        public string Name => "Favorable current";
        public string Desc => "Significantly boost submarine's speed for short time";
        public Sprite Icon => null;
        
        public void Execute()
        {
            // TODO
        }
    }
}