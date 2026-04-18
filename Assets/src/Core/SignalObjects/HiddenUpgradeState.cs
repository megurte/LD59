using System.Collections.Generic;
using Constants;
using Core.Fish;
using Core.Submarine;
using Core.Upgrade;
using GlobalSpace;
using UnityEngine;

namespace Core.SignalObjects
{
    public class HiddenUpgradeState : MonoBehaviour, IHookable, IDamageable
    {
        [SerializeField] private float weight = 1;
        [SerializeField] private float fuel = 8;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform hookTransform;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public float Weight => weight;
        public float FuelAmount => fuel;
        public Transform RootTransform => rootTransform;
        public Transform HookTransform => hookTransform;
        public void OnHook()
        {
            _spriteRenderer.gameObject.SetActive(true);
        }
        
        public void OnObtain()
        {
            var sample = new List<IUpgrade>{ new SampleUpgrade(), new SampleUpgrade(), new SampleUpgrade()};
            Global.UpgradeSelectorController.ShowUpgradeSelector(sample);
            gameObject.SetActive(false);
        }

        public void OnTakeDamage(float damage)
        {
            var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(pfb, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}