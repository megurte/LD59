using System;
using Constants;
using Common;
using Core.Fish;
using Core.Submarine;
using DG.Tweening;
using GlobalSpace;
using UnityEngine;

namespace Core.SignalObjects
{
    public class HiddenMineState : MonoBehaviour, IDamageable, IHookable, ITakeable
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

        private bool _isTriggered;

        private void OnEnable()
        {
            _isTriggered = false;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.gameObject.SetActive(false);
            }

            if (signal != null)
            {
                signal.SetActive(true);
            }
        }

        public void OnHook()
        {
            TriggerMine();
        }

        public void OnTakeDamage(float damage)
        {
            var bubbleBurst = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(bubbleBurst, transform.position, Quaternion.identity);
            GameAudio.PlayExplosion(0.5f, 0.94f, 1.02f);
            GameAudio.PlayBubbleSpawn(0.2f, 0.92f, 1.02f);
            _spriteRenderer.gameObject.SetActive(true);
            signal.SetActive(false);
            _spriteRenderer.DOFade(0, 1.4f).SetEase(Ease.OutSine).OnComplete(() => Destroy(gameObject));
        }

        public void OnObtain()
        {
        }

        public void Take(SubmarineMovementController submarineMovement)
        {
            TriggerMine();
        }

        private void TriggerMine()
        {
            if (_isTriggered)
            {
                return;
            }

            _isTriggered = true;

            Global.SubmarineMovement.SubstructFuel(fuelSub);
            var bubbleBurst = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(bubbleBurst, transform.position, Quaternion.identity);
            GameAudio.PlayExplosion(0.55f, 0.94f, 1.02f);
            GameAudio.PlayBubbleSpawn(0.2f, 0.92f, 1.02f);
            _spriteRenderer.gameObject.SetActive(true);
            signal.SetActive(false);
            _spriteRenderer.DOFade(0, 0.3f).SetEase(Ease.OutSine).OnComplete(() => Destroy(gameObject));
        }
    }
}
