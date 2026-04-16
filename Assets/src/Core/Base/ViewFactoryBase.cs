using UnityEngine;

namespace Core.Base
{
    public abstract class ViewFactoryBase<TView> where TView : Component  
    {
        private TView _prefab;

        protected abstract string PrefabPath { get; }

        protected TView Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = Resources.Load<TView>(PrefabPath);
#if UNITY_EDITOR
                    if (_prefab == null)
                        Debug.LogError($"{GetType().Name}: prefab not found at '{PrefabPath}'");
#endif
                }
                return _prefab;
            }
        }

        public void ClearCache() { _prefab = null; }
    }
}