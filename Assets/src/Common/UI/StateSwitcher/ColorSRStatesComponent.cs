using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class ColorSRStatesComponent : StatesComponent<ColorSRStatesHolder>
    {
        public Color GetColor(string stateName, SpriteRenderer target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}