using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Base
{
    [Serializable]
    public class CompositeStateCard
    {
        
    }

    public class CompositeState
    {
        public List<CompositeStateCard> components = new(4);

        public T Get<T>() where T : CompositeStateCard, new()
        {
            foreach (var comp in components)
            {
                if (comp is T ct)
                    return ct;
            }

            var newState = new T();
            components.Add(newState);
            return newState;
        }
    }
    
    [Serializable]
    public class ObjectState : CompositeState
    {
        public CMSEntity model;
        public Interactable view;

        public void DestroySafelyView()
        {
            if (view != null)
            {
                GameObject.Destroy(view.gameObject);
                view = null;
            }
        }

        public bool Is<T>() where T : EntityComponentDefinition, new()
        {
            return Is<T>(out _);
        }
        
        public bool Is<T>(out T t) where T : EntityComponentDefinition, new()
        {
            return Is<T>(out t);
        }

        public string GetName()
        {
            return model.Is<TagName>(out var name) ? name.name : string.Empty;
        }
    }

    [Serializable]
    public class TagName : EntityComponentDefinition
    {
        public string name;
    }
}