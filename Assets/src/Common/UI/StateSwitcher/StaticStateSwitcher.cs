using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.StateSwitcher
{
    public class StaticStateSwitcher : StateSwitcher
    {
        [SerializeField] private ColorStatesHolder[] _colorStates;
        [SerializeField] private EnableStatesHolder[] _enableStates;
        [SerializeField] private AlphaStatesHolder[] _alphaStates;
        [SerializeField] private MaterialParamStatesHolder[] _materialStates;
        [SerializeField] private FontStatesHolder[] _fontStates;
        [SerializeField] private RotateStatesHolder[] _rotateStates;

        [SerializeField] private string _testState;
        
        public override void SwitchState(string state)
        {
            ApplyState(_enableStates, state);
            ApplyState(_colorStates, state);
            ApplyState(_alphaStates, state);
            ApplyState(_materialStates, state);
            ApplyState(_fontStates, state);
            ApplyState(_rotateStates, state);
        }

        public Color GetColor(string stateName, Graphic target)
        {
            return GetValueFromTargetStates(_colorStates, target, stateName);
        }
        
        public bool IsObjectEnable(string stateName, GameObject target)
        {
            return GetValueFromTargetStates(_enableStates, target, stateName);
        }
        
        public float GetAlpha(string stateName, CanvasGroup target)
        {
            return GetValueFromTargetStates(_alphaStates, target, stateName);
        }

        [ContextMenu("Test")]
        public void ApplyTestState()
        {
            SwitchState(_testState);
        }

        private void ApplyState<TTarget, TValue, TState>(IEnumerable<StatesHolder<TTarget, TValue, TState>> targetStates, string stateName)
            where TState : AbstractState<TTarget, TValue>
        {
            foreach (var s in targetStates)
            {
                if (s.IsActive())
                {
                    s.Apply(stateName);
                }
            }
        }
        
        private TValue GetValueFromTargetStates<TTarget, TValue, TState>(IEnumerable<StatesHolder<TTarget, TValue, TState>> targetStates, TTarget target, string stateName) where TState : AbstractState<TTarget, TValue>
        {
            var t = targetStates.FirstOrDefault(x => x.Target.Equals(target));
            if (t == null)
            {
                throw new ArgumentOutOfRangeException(stateName);
            }

            return t.GetValue(stateName);
        }
    }
}