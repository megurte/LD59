using Constants;
using Core.Submarine;
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
            if (Global.GameProgress.PlayerState.mineProjectiles)
            {
                TrySpawnFuelPickup();
                var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
                Instantiate(pfb, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }

        private void TrySpawnFuelPickup()
        {
            if (_hasDroppedFuelPickup
                || Global.GameProgress == null
                || !Global.GameProgress.PlayerState.fishFuelDrop)
            {
                return;
            }

            _hasDroppedFuelPickup = true;

            var spawnOffset = (Vector3)(Random.insideUnitCircle * FuelDropSpawnJitter);
            var driftDirection = Random.insideUnitCircle;
            if (driftDirection.sqrMagnitude <= 0.0001f)
            {
                driftDirection = Vector2.up;
            }

            /*FuelPickup.Spawn(
                transform.position + spawnOffset,
                fuel,
                driftDirection.normalized * FuelDropDriftSpeed);*/
        }
    }
}
