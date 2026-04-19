using Constants;
using Core.Submarine;
using GlobalSpace;
using UnityEngine;

namespace Core.Fish
{
    public class FishState : MonoBehaviour, IHookable, IDamageable
    {
        public bool IsHooked { get; private set; }
        
        [SerializeField] private float weight = 1;
        [SerializeField] private float fuel = 8;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform hookTransform;

        public float Weight => weight;
        public float FuelAmount => fuel;
        public Transform RootTransform => rootTransform;
        public Transform HookTransform => hookTransform;
        
        public void OnHook()
        {
            var movement = GetComponent<FishMovementController>();
            movement.BeginEscape();
            IsHooked = true;
        }

        public void OnObtain()
        {
            gameObject.SetActive(false);
        }

        public void OnTakeDamage(float damage)
        {
            if (Global.GameProgress.PlayerState.mineProjectiles)
            {
                var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
                Instantiate(pfb, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}