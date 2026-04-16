using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class ScaleStateComponent : StatesComponent<ScaleStateHolder>
    {
        public Vector3 GetScale(string stateName, RectTransform target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}


