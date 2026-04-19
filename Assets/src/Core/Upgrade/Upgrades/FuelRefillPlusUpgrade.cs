using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class FuelRefillPlusUpgrade : UpgradeBase
    {
        public override string Name => "Oil Barrel+";
        public override string Desc => "Refills the submarine's fuel by 50%";
        public override Sprite Icon => Resources.Load<Sprite>("CMS/Sprites/america");
        
        public override void Execute()
        {
            Global.SubmarineMovement.AddFuel(50);
        }
    }
}
