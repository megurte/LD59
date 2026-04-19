using System;
using Constants;
using Core.Fish;
using Core.Submarine;
using GlobalSpace;
using UnityEngine;

namespace Core.SignalObjects
{
    public class HiddenMineState : MonoBehaviour, IDamageable, IHookable
    {
        [SerializeField] private float weight = 1;
        [SerializeField] private float fuel = 8;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform hookTransform;
        [SerializeField] private float fuelSub;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GameObject signal;

        public float Weight => weight;
        public float FuelAmount => fuel;
        public Transform RootTransform => rootTransform;
        public Transform HookTransform => hookTransform;

        private void OnEnable()
        {
            _spriteRenderer.gameObject.SetActive(false);
            signal.SetActive(true);
        }

        public void OnHook()
        {
            Global.SubmarineMovement.SubstructFuel(fuelSub);
            var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(pfb, transform.position, Quaternion.identity);
            _spriteRenderer.gameObject.SetActive(true);
            signal.SetActive(false);
            Destroy(gameObject);
        }

        public void OnTakeDamage(float damage)
        {
            var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(pfb, transform.position, Quaternion.identity);
            _spriteRenderer.gameObject.SetActive(true);
            signal.SetActive(false);
            Destroy(gameObject);
        }

        public void OnObtain()
        {
        }
    }
}