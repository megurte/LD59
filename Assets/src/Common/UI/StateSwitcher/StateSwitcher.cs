using System;
using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public abstract class StateSwitcher : MonoBehaviour
    {
        public void SwitchState(Enum state)
        {
            SwitchState(state.ToString());
        }
        public abstract void SwitchState(string state);
    }
}