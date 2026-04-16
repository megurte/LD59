using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.StateSwitcher
{
    public class ColorStatesComponent : StatesComponent<ColorStatesHolder>
    {
        public Color GetColor(string stateName, Graphic target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}