using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade.Upgrades
{
    public class SubmarineSpeedBoostUpgrade : UpgradeBase
    {
        private const float SpeedMultiplier = 5.5f;

        public override string Name => "Favorable current";
        public override string Desc => "Significantly boosts submarine's speed for short time";
        public override Sprite Icon => Resources.Load<Sprite>("CMS/Sprites/current");

        public override void Execute()
        {
            if (Global.SubmarineMovement == null)
            {
                return;
            }

            var duration = Global.GameProgress.PlayerState.speedBoostTime;
            Global.SubmarineMovement.ApplyTemporarySpeedBoost(SpeedMultiplier, duration);
            Global.SubmarineCameraController?.PlaySpeedBoostState(duration);
        }
    }
}
