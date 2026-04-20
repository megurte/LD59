using Constants;
using Core.Submarine;
using Common;
using GlobalSpace;
using UnityEngine;

namespace Core.Fish
{
    public class FishState : MonoBehaviour, IHookable, IDamageable
    {
        private const float FuelDropSpawnJitter = 0.2f;
        private const float FuelDropDriftSpeed = 1.1f;

        public bool IsHooked { get; private set; }
        
        [SerializeField] private float weight = 1;
        [SerializeField] private float fuel = 8;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform hookTransform;

        private bool _hasDroppedFuelPickup;

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
            DropExtract();
            
            var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(pfb, transform.position, Quaternion.identity);
            GameAudio.PlayBubbleSpawn(0.16f, 0.96f, 1.06f);
            Destroy(gameObject);
        }

        private void DropExtract()
        {
            if (Global.GameProgress.PlayerState.availableDropFromFish)
            {
                var pfb = Global.EffectFactory.LoadAndCreate<GameObject>(Models.Extract);
                pfb.transform.position = transform.position;
            }
        }
    }
}
