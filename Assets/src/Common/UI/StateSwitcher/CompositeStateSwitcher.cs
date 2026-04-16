using System;
using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public class CompositeStateSwitcher : StateSwitcher
    {
        [SerializeField]
        private StatesComponent[] _states;

        [SerializeField]
        private string _testState;

        public bool ForceChangeState = false;
        
        private string _currentState;

        [ContextMenu("Test")]
        public void ApplyTestState()
        {
            SwitchState(_testState);
        }

        public override void SwitchState(string stateName)
        {
            //if (_currentState != stateName) //todo: in some cases it cause bug
            {
                foreach (var s in _states)
                {
                    try
                    {
                        if (ForceChangeState)
                        {
                            s.ForceChangeState = true;
                        }
                        s.Apply(stateName);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[CompositeStateSwitcher] can't switch to style: {stateName}. Reason: {e} StackTrace: {e.StackTrace}", this);
                    }
                }
                _currentState = stateName;
            }
        }
    }
}