using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class EnableStatesComponent : StatesComponent<EnableStatesHolder>
    {
        public bool IsObjectEnable(string stateName, GameObject target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}