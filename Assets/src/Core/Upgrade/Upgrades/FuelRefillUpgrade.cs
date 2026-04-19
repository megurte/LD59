using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class FuelRefillUpgrade : UpgradeBase
    {
        public override string Name => "Oil Barrel";
        public override string Desc => "Refills the submarine's fuel by 30%";
        public override Sprite Icon => null;
        
        public override void Execute()
        {
            Global.SubmarineMovement.AddFuel(30);
        }
    }
}
