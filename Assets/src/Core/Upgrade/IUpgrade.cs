using UnityEngine;

namespace Core.Upgrade
{
    public interface IUpgrade
    {
        string Name { get; }
        string Desc { get; }
        Sprite Icon { get; }

        void Execute();
    }
}