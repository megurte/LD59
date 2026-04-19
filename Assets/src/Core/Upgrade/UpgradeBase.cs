using UnityEngine;

namespace Core.Upgrade
{
    public abstract class UpgradeBase : IUpgrade
    {
        public abstract string Name { get; }
        public abstract string Desc { get; }
        public virtual Sprite Icon => null;
        public virtual ICondition Condition => new EmptyCondition();

        public abstract void Execute();
    }
    
    public class EmptyCondition : ICondition
    {
        public bool IsSatisfied() => true;
    }
}
