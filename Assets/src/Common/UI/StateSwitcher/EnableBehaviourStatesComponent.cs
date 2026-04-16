using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class EnableBehaviourStatesComponent : StatesComponent<EnableBehaviourStateHolder>
    {
        public bool IsObjectEnable(string stateName, Behaviour target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}