using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class AlphaStatesComponent : StatesComponent<AlphaStatesHolder>
    {
        public float GetAlpha(string stateName, CanvasGroup target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}