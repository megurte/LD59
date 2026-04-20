using System.Collections.Generic;
using System.Globalization;
using Common;
using Constants;
using DG.Tweening;
using GlobalSpace;
using TMPro;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineMovementController : MonoBehaviour
    {
        private static readonly int HitEffectBlendProperty = Shader.PropertyToID("_HitEffectBlend");
        private static readonly int HitEffectColorProperty = Shader.PropertyToID("_HitEffectColor");

        private struct MilestoneData
        {
            public float Progress;
            public float SortProgress;
        }

        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private Transform milestoneRoot;
        [SerializeField] private Vector2 takeableCollectorOffset = new(0f, 0.1f);
        [SerializeField] [Min(0.1f)] private float takeableCollectorRadius = 2.4f;
        [SerializeField] private Canvas fuelPopupCanvas;
        [SerializeField] private TextMeshProUGUI fuelPopupTemplate;
        [SerializeField] private Vector2 fuelPopupSpawnJitter = new(12f, 4f);
        [SerializeField] private float fuelPopupFontSize = 30f;
        [SerializeField] private float fuelPopupRiseDistance = 48f;
        [SerializeField] private float fuelPopupFadeDelay = 0.15f;
        [SerializeField] private float fuelPopupFadeDuration = 0.65f;
        [SerializeField] private Color fuelPopupGainColor = new(0.49f, 0.96f, 0.7f, 1f);
        [SerializeField] private Color fuelPopupLossColor = new(1f, 0.42f, 0.42f, 1f);
        [SerializeField] private Color fuelPopupOutlineColor = new(0.05f, 0.08f, 0.12f, 0.9f);
        [SerializeField] private float damageFlashDuration = 0.08f;
        [SerializeField] [Range(0f, 1f)] private float damageFlashPeakBlend = 0.09f;
        [SerializeField] private Color damageFlashColor = Color.white;
        [SerializeField] private SpriteRenderer[] damageFlashRenderers;
        [SerializeField] private Vector3 moveDirection = Vector3.right;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float startFuel = 100f;
        [SerializeField] private float fuelBurnPerSecond = 1f;
        [SerializeField] private SubmarineTakeableCollector takeableCollector;
            
        private float _currentFuel;
        private float _speedBoostEndTime;
        private float _speedBoostMultiplier = 1f;
        private bool _isMovementStopped;
        private bool _milestonesDirty = true;
        private readonly List<MilestoneData> _milestones = new();
        private readonly List<Tween> _damageFlashTweens = new();
        private ParticleSystem[] _attachedParticleSystems;

        public float CurrentFuel => _currentFuel;
        public float FuelNormalized => maxFuel <= 0f ? 0f : _currentFuel / maxFuel;
        public float RouteProgressNormalized => GetRouteProgress();
        public bool HasReachedRouteEnd => startPoint != null && endPoint != null && RouteProgressNormalized >= 0.9999f;
        public bool IsTemporarySpeedBoostActive => Time.time <= _speedBoostEndTime && _speedBoostMultiplier > 1f;
        public int MilestoneCount
        {
            get
            {
                EnsureMilestonesCache();
                return _milestones.Count;
            }
        }

        public int CurrentMilestoneIndex => GetCurrentMilestoneIndex();

        private void Awake()
        {
            Global.SubmarineMovement = this;
            _currentFuel = Mathf.Clamp(startFuel, 0f, maxFuel);
            _milestonesDirty = true;
            RefreshTakeableCollector();
            ResolveDamageFlashRenderers();
            ResolveAttachedParticleSystems();
        }

        private void Update()
        {
            if (_isMovementStopped || _currentFuel <= 0f)
            {
                return;
            }

            var direction = moveDirection.normalized;
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            if (HasReachedEnd(direction))
            {
                StopMovement();
                return;
            }

            transform.position += direction * GetCurrentMoveSpeed() * Time.deltaTime;
            _currentFuel = Mathf.Max(0f, _currentFuel - fuelBurnPerSecond * Time.deltaTime);

            if (HasReachedEnd(direction))
            {
                StopMovement();
            }
        }

        public void AddFuel(float amount)
        {
            GameAudio.PlayPickUp(2f);
            ApplyFuelDelta(Mathf.Abs(amount));
        }

        public void SubstructFuel(float amount)
        {
            ApplyFuelDelta(-Mathf.Abs(amount));
        }

        public void StopMovement()
        {
            _isMovementStopped = true;
            StopAttachedParticleSystems();

            if (HasReachedRouteEnd && endPoint != null)
            {
                transform.position = endPoint.position;
            }
        }

        public void SetTakeableCollectionEnabled(bool isEnabled)
        {
            RefreshTakeableCollector();
            if (takeableCollector != null)
            {
                takeableCollector.enabled = isEnabled;
            }
        }

        public void ApplyTemporarySpeedBoost(float multiplier, float duration)
        {
            _speedBoostMultiplier += multiplier;
            _speedBoostEndTime = Mathf.Max(_speedBoostEndTime, Time.time + duration);
        }

        public float GetMilestoneProgressNormalized(int milestoneIndex)
        {
            EnsureMilestonesCache();
            if (milestoneIndex < 0 || milestoneIndex >= _milestones.Count)
            {
                return 0f;
            }

            return _milestones[milestoneIndex].Progress;
        }

        private bool HasReachedEnd(Vector3 direction)
        {
            if (startPoint == null || endPoint == null)
            {
                return false;
            }

            return Vector3.Dot(endPoint.position - transform.position, direction) <= 0f;
        }

        private float GetRouteProgress()
        {
            if (startPoint == null || endPoint == null)
            {
                return 0f;
            }

            var line = endPoint.position - startPoint.position;
            if (line.sqrMagnitude <= 0f)
            {
                return 0f;
            }

            var offset = transform.position - startPoint.position;
            return Mathf.Clamp01(Vector3.Dot(offset, line) / line.sqrMagnitude);
        }

        private float GetCurrentMoveSpeed()
        {
            return moveSpeed * GetSpeedBoostMultiplier();
        }

        private float GetSpeedBoostMultiplier()
        {
            if (Time.time > _speedBoostEndTime)
            {
                _speedBoostMultiplier = 1f;
                return 1f;
            }

            return _speedBoostMultiplier;
        }

        private int GetCurrentMilestoneIndex()
        {
            EnsureMilestonesCache();
            if (_milestones.Count == 0)
            {
                return 0;
            }

            var currentProgress = RouteProgressNormalized;
            for (var i = _milestones.Count - 1; i >= 0; i--)
            {
                if (currentProgress + 0.0001f >= _milestones[i].Progress)
                {
                    return i;
                }
            }

            return 0;
        }

        private void EnsureMilestonesCache()
        {
            if (!_milestonesDirty)
            {
                return;
            }

            _milestonesDirty = false;
            _milestones.Clear();

            if (startPoint == null)
            {
                return;
            }

            _milestones.Add(new MilestoneData
            {
                Progress = 0f,
                SortProgress = float.NegativeInfinity
            });

            if (milestoneRoot == null || !TryGetRouteData(out var routeOrigin, out var routeLine, out var routeLengthSqr))
            {
                return;
            }

            for (var i = 0; i < milestoneRoot.childCount; i++)
            {
                var milestone = milestoneRoot.GetChild(i);
                if (milestone == null || milestone == startPoint)
                {
                    continue;
                }

                var rawProgress = Vector3.Dot(milestone.position - routeOrigin, routeLine) / routeLengthSqr;
                if (rawProgress <= 0f || rawProgress > 1f)
                {
                    continue;
                }

                _milestones.Add(new MilestoneData
                {
                    Progress = Mathf.Clamp01(rawProgress),
                    SortProgress = rawProgress
                });
            }

            _milestones.Sort((left, right) => left.SortProgress.CompareTo(right.SortProgress));
        }

        private bool TryGetRouteData(out Vector3 routeOrigin, out Vector3 routeLine, out float routeLengthSqr)
        {
            routeOrigin = startPoint != null ? startPoint.position : Vector3.zero;
            routeLine = endPoint != null && startPoint != null ? endPoint.position - startPoint.position : Vector3.zero;
            routeLengthSqr = routeLine.sqrMagnitude;
            return routeLengthSqr > 0f;
        }

        private void OnValidate()
        {
            _milestonesDirty = true;
            RefreshTakeableCollector();
            ResolveDamageFlashRenderers();
            ResolveAttachedParticleSystems();
        }

        private void OnDisable()
        {
            StopDamageFlash();
        }

        private void RefreshTakeableCollector()
        {
            takeableCollector ??= GetComponentInChildren<SubmarineTakeableCollector>(true);
            if (takeableCollector == null)
            {
                return;
            }

            takeableCollector.transform.localPosition = new Vector3(takeableCollectorOffset.x, takeableCollectorOffset.y, 0f);

            var collectorCollider = takeableCollector.GetComponent<CircleCollider2D>();
            if (collectorCollider != null)
            {
                collectorCollider.radius = takeableCollectorRadius;
            }
        }

        private void ResolveAttachedParticleSystems()
        {
            _attachedParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        private void StopAttachedParticleSystems()
        {
            ResolveAttachedParticleSystems();
            if (_attachedParticleSystems == null || _attachedParticleSystems.Length == 0)
            {
                return;
            }

            for (var i = 0; i < _attachedParticleSystems.Length; i++)
            {
                _attachedParticleSystems[i]?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void ApplyFuelDelta(float delta)
        {
            if (Mathf.Abs(delta) <= 0.0001f)
            {
                return;
            }

            var previousFuel = _currentFuel;
            _currentFuel = Mathf.Clamp(_currentFuel + delta, 0f, maxFuel);
            var appliedDelta = _currentFuel - previousFuel;

            if (Mathf.Abs(appliedDelta) > 0.0001f)
            {
                if (appliedDelta < 0f)
                {
                    PlayDamageFlash();
                }

                SpawnFuelPopup(appliedDelta);
            }
        }

        private void ResolveDamageFlashRenderers()
        {
            if (damageFlashRenderers != null && damageFlashRenderers.Length > 0)
            {
                return;
            }

            damageFlashRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void PlayDamageFlash()
        {
            if (damageFlashDuration <= 0f || damageFlashPeakBlend <= 0f)
            {
                return;
            }

            ResolveDamageFlashRenderers();
            StopDamageFlash();

            var halfDuration = Mathf.Max(0.01f, damageFlashDuration * 0.5f);
            foreach (var spriteRenderer in damageFlashRenderers)
            {
                if (spriteRenderer == null)
                {
                    continue;
                }

                var material = spriteRenderer.material;
                if (material == null || !material.HasProperty(HitEffectBlendProperty))
                {
                    continue;
                }

                if (material.HasProperty(HitEffectColorProperty))
                {
                    material.SetColor(HitEffectColorProperty, damageFlashColor);
                }

                material.SetFloat(HitEffectBlendProperty, 0f);

                var blendInTween = DOTween.To(
                        () => material.GetFloat(HitEffectBlendProperty),
                        value => material.SetFloat(HitEffectBlendProperty, value),
                        damageFlashPeakBlend,
                        halfDuration)
                    .SetEase(Ease.OutSine);

                var blendOutTween = DOTween.To(
                        () => material.GetFloat(HitEffectBlendProperty),
                        value => material.SetFloat(HitEffectBlendProperty, value),
                        0f,
                        halfDuration)
                    .SetEase(Ease.InSine);

                var flashSequence = DOTween.Sequence()
                    .Append(blendInTween)
                    .Append(blendOutTween)
                    .OnKill(() =>
                    {
                        if (material != null)
                        {
                            material.SetFloat(HitEffectBlendProperty, 0f);
                        }
                    });

                _damageFlashTweens.Add(flashSequence);
            }
        }

        private void StopDamageFlash()
        {
            if (_damageFlashTweens.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _damageFlashTweens.Count; i++)
            {
                _damageFlashTweens[i]?.Kill();
            }

            _damageFlashTweens.Clear();
        }

        private void SpawnFuelPopup(float delta)
        {
            if (fuelPopupCanvas == null || fuelPopupTemplate == null)
            {
                return;
            }

            var popupText = Instantiate(fuelPopupTemplate, fuelPopupCanvas.transform);
            var popupObject = popupText.gameObject;
            popupObject.name = "FuelDeltaPopup";
            if (popupObject.activeSelf)
            {
                popupObject.SetActive(false);
            }

            var popupTransform = popupText.rectTransform;
            popupTransform.anchoredPosition = fuelPopupTemplate.rectTransform.anchoredPosition + new Vector2(
                Random.Range(-fuelPopupSpawnJitter.x, fuelPopupSpawnJitter.x),
                Random.Range(-fuelPopupSpawnJitter.y, fuelPopupSpawnJitter.y));

            popupText.text = FormatFuelDelta(delta);
            popupText.raycastTarget = false;

            var popupLifetime = Mathf.Max(fuelPopupFadeDelay + fuelPopupFadeDuration, 0.2f) + 0.1f;
            var popupAutoDestroy = popupObject.GetComponent<AutoDestroy>();
            if (popupAutoDestroy != null)
            {
                popupAutoDestroy.SetLifetime(popupLifetime);
            }

            popupObject.SetActive(true);

            popupTransform
                .DOAnchorPosY(popupTransform.anchoredPosition.y + fuelPopupRiseDistance, popupLifetime)
                .SetEase(Ease.OutSine);

            popupText
                .DOFade(0f, fuelPopupFadeDuration)
                .SetDelay(fuelPopupFadeDelay)
                .SetEase(Ease.OutSine);
        }

        private static string FormatFuelDelta(float delta)
        {
            var absoluteDelta = Mathf.Abs(delta);
            var roundedDelta = Mathf.Round(absoluteDelta);
            var value = Mathf.Abs(absoluteDelta - roundedDelta) <= 0.01f
                ? roundedDelta.ToString(CultureInfo.InvariantCulture)
                : absoluteDelta.ToString("0.#", CultureInfo.InvariantCulture);

            return delta >= 0f ? $"+{value}" : $"-{value}";
        }
    }
}
