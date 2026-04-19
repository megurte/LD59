using Constants;
using Core.Fish;
using Core.Submarine;
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
            _spriteRenderer.gameObject.SetActive(true);
            signal.SetActive(false);
        }

        public void OnObtain()
        {
            var sample = Global.UpgradeDropService.GetRandomUpgrades(3);
            if (sample.Count != 3)
            {
                Debug.LogError("HiddenUpgradeState failed to collect 3 unique upgrades.");
                return;
            }

            Global.UpgradeSelectorController.ShowUpgradeSelector(sample);
            Destroy(gameObject);
        }

        public void OnTakeDamage(float damage)
        {
            var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(pfb, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
