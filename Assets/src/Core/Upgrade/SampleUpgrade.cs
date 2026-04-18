using UnityEngine;

namespace Core.Upgrade
{
    public class SampleUpgrade : IUpgrade
    {
        public string Name => "Sample";
        public string Desc => "Just a sample upgrade";
        public Sprite Icon => null;
        public void Execute()
        {
            Debug.Log("Sample upgrade executed");
        }
    }
}