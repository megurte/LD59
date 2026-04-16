using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.UI.StateSwitcher
{
    public abstract class StatesComponent : MonoBehaviour
    {
        public bool ForceChangeState = false;
        public abstract void Apply(string stateName);
#if UNITY_EDITOR
        protected abstract string IsValidState();
#endif
    }

    public abstract class StatesComponent<T>: StatesComponent
        where T : StatesHolder
    {
        [SerializeField] private T[] _states;
        public T[] States { get { return _states; } }

#if UNITY_EDITOR
      
        //Call with reflection from StatesComponentEditor
        protected override string IsValidState()
        {
            if (_states == null)
                return string.Empty;
            
            var log = string.Empty;
            foreach (var state in _states)
            {
                if(state.TargetGameObject == null)
                    continue;
                
                var parentWithStateSwitcher = GetParentWithStateSwitcher(state);
                if (gameObject != parentWithStateSwitcher)
                {
                    var targetName = state.TargetGameObject.name;
                    var error = parentWithStateSwitcher != null
                        ? $"'{targetName}' is a child of another StateSwitcher - {parentWithStateSwitcher.gameObject.name}"
                        : $"'{targetName}' is not a child of any StateSwitcher";

                    log = $"{gameObject.name} - {GetType()} has a reference to '{targetName}', but {error}. Fix it! ";
                }
            }

            return log;
        }

        private GameObject GetParentWithStateSwitcher(T state)
        {
            Common.UI.StateSwitcher.StateSwitcher result = null;
            
            var target = state.TargetGameObject.transform;
            
            while (result == null && target != null)
            {
                if(target != state.TargetGameObject.transform || target == transform)
                    result = target.GetComponent<Common.UI.StateSwitcher.StateSwitcher>();
                target = target.transform.parent;
            }

            return result? result.gameObject : null;
        }
#endif

        public override void Apply(string stateName)
        {
            foreach (var s in _states)
            {
                if (s.IsActive() || ForceChangeState)
                {
                    s.Apply(stateName);
                }
            }
        }

        protected TValue GetValueFromTargetStates<TTarget, TValue, TState>(IEnumerable<StatesHolder<TTarget, TValue, TState>> targetStates, TTarget target, string stateName) where TState : AbstractState<TTarget, TValue>
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