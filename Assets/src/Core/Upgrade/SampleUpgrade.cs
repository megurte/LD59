using UnityEngine;

namespace Core.Upgrade
{
    public class SampleUpgrade : UpgradeBase
    {
        public override string Name => "Sample";
        public override string Desc => "Just a sample upgrade";
        public override Sprite Icon => null;

        public override void Execute()
        {
            Debug.Log("Sample upgrade executed");
        }
    }
}
