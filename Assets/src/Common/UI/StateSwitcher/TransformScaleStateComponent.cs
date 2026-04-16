using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class TransformScaleStateComponent : StatesComponent<TransformScaleStateHolder>
    {
        public Vector3 GetScale(string stateName, Transform target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}