using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class FuelRefillUpgrade : IUpgrade
    {
        public string Name => "Oil Barrel";
        public string Desc => "Refills the submarine's fuel by 30%";
        public Sprite Icon => null;
        
        public void Execute()
        {
            Global.SubmarineMovement.AddFuel(30);
        }
    }
}