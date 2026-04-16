using UnityEngine;

namespace Core.Base
{
    public class Interactable : MonoBehaviour
    {
        protected bool interactable = true;

        public virtual void SetInteractable(bool state) => interactable = state;
        
        public T As<T>() where T : Interactable
        {
            return this as T;
        }
    }
}