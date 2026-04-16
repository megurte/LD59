using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class SpriteStatesComponent : StatesComponent<SpriteStateHolder>
    {
        public Sprite GetSprite(string stateName, SpriteRenderer target)
        {
            return GetValueFromTargetStates(States, target, stateName);
        }
    }
}