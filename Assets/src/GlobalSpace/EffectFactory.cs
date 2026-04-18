using System;
using Core.Base;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GlobalSpace
{
    [Serializable]
    public class TagVFX : EntityComponentDefinition
    {
        public GameObject vfx;
    }
    
    public class EffectFactory
    {
        public GameObject Create(ObjectState owner, Vector3 pos)
        {
            var vfx = owner.model.Get<TagVFX>().vfx;
            return Object.Instantiate(vfx, pos, new Quaternion());
        }
        
        public GameObject Create(ParticleSystem effect, Vector3 pos)
        {
            return Object.Instantiate(effect.gameObject, pos, new Quaternion());
        }
        
        public T Create<T>(T effect) where T : Object
        {
            return Object.Instantiate(effect);
        }
        
        public ParticleSystem LoadVFX(string path)
        {
            return Resources.Load<ParticleSystem>(path);
        }
        
        public T LoadVFX<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public GameObject LoadAndCreate(string path, Vector3 pos)
        {
            var eff = LoadVFX(path);
            return Create(eff, pos);
        }
        
        public T LoadAndCreate<T>(string path) where T : Object
        {
            var eff = LoadVFX<T>(path);
            var pfb = Create<T>(eff);
            //pfb.GetComponent<Transform>().position = pos;
            return pfb;
        }
    }
}