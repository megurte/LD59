using Common;
using Constants;
using Core.Submarine;
using GlobalSpace;
using UnityEngine;

namespace Core.Fish
{
    public class FishSphereState : MonoBehaviour, IHookable, IDamageable
    {
        [SerializeField] private float weight = 1.35f;
        [SerializeField] private float fuel = 10f;
        [SerializeField] private float contactFuelDamage = 12f;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform hookTransform;

        private FishSphereMovementController _movementController;
        private bool _isConsumed;

        public bool IsHooked { get; private set; }
        public float Weight => weight;
        public float FuelAmount => fuel;
        public Transform RootTransform => rootTransform;
        public Transform HookTransform => hookTransform;

        private void Awake()
        {
            rootTransform ??= transform;
            hookTransform ??= transform;
            _movementController = GetComponent<FishSphereMovementController>();
        }

        public void OnHook()
        {
            if (_isConsumed)
            {
                return;
            }

            IsHooked = true;
            _movementController?.StopMovement();
        }

        public void OnObtain()
        {
            _isConsumed = true;
            gameObject.SetActive(false);
        }

        public void OnTakeDamage(float damage)
        {
            if (_isConsumed)
            {
                return;
            }

            _isConsumed = true;
            _movementController?.StopMovement();
            DropExtract();
            PlayBurst(0.16f, 0.96f, 1.08f);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isConsumed || IsHooked || other == null)
            {
                return;
            }

            var submarine = other.GetComponentInParent<SubmarineMovementController>();
            if (submarine == null || submarine.CurrentFuel <= 0f || submarine.HasReachedRouteEnd)
            {
                return;
            }

            _isConsumed = true;
            _movementController?.StopMovement();
            submarine.SubstructFuel(contactFuelDamage);
            PlayBurst(0.2f, 0.92f, 1.02f);
            Destroy(gameObject);
        }

        private void DropExtract()
        {
            if (Global.GameProgress?.PlayerState.availableDropFromFish != true)
            {
                return;
            }

            var extract = Global.EffectFactory.LoadAndCreate<GameObject>(Models.Extract);
            if (extract != null)
            {
                extract.transform.position = transform.position;
            }
        }

        private void PlayBurst(float volume, float minPitch, float maxPitch)
        {
            var bubbleBurst = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            if (bubbleBurst != null)
            {
                Instantiate(bubbleBurst, transform.position, Quaternion.identity);
            }

            GameAudio.PlayBubbleSpawn(volume, minPitch, maxPitch);
        }
    }
}
