using Constants;
using DG.Tweening;
using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineHarpoonController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform harpoonPivot;
        [SerializeField] private Transform harpoon;
        [SerializeField] private Rigidbody2D harpoonBody;
        [SerializeField] private Collider2D harpoonCollider;
        [SerializeField] private SpriteRenderer harpoonRenderer;
        [SerializeField] private LineRenderer rope;
        [SerializeField] private SubmarineMovementController submarineMovement;
        [SerializeField] private float fireSpeed = 16f;
        [SerializeField] private float returnSpeed = 12f;
        [SerializeField] private float minReturnSpeed = 4f;
        [SerializeField] private float weightFactor = 0.45f;
        [SerializeField] private float maxDistance = 16f;
        [SerializeField] private float ropeWidth = 0.08f;
        [SerializeField] [Min(2)] private int ropeSegmentCount = 14;
        [SerializeField] private float ropeSlack = 0.075f;
        [SerializeField] private float ropeWaveAmplitude = 0.18f;
        [SerializeField] private float ropeWaveFrequency = 3.6f;
        [SerializeField] private float ropeWaveTravelSpeed = 7.5f;
        [SerializeField] private float ropeTensionLerpSpeed = 11f;
        [SerializeField] private float returnRopeWidthMultiplier = 1.18f;
        [SerializeField] [Range(0f, 1f)] private float ropeGravityBlend = 0.45f;
        [SerializeField] private float harpoonScale = 1f;
        [SerializeField] private float harpoonFlightStretch = 0.04f;
        [SerializeField] private float harpoonReturnStretch = 0.12f;
        [SerializeField] private float harpoonStretchLerpSpeed = 14f;
        [SerializeField] private LayerMask hookMask = ~0;

        public bool InAir {get; private set;}
        
        private Vector3 _harpoonBaseScale;
        private Vector3 _harpoonPosition;
        private Vector3 _targetPosition;
        private Vector3 _lastDirection = Vector3.up;
        private float _ropeTensionBlend;
        private float _harpoonScaleAnimationEndTime;

        private IHookable _hookedTarget;
        private Transform _hookedRoot;
        private Transform _hookedPoint;
        private Vector3 _hookOffset;

        private HarpoonState _state;

        private enum HarpoonState
        {
            Idle,
            Flying,
            Returning
        }

        private void Awake()
        {
            Global.HarpoonController = this;
            harpoonRenderer.sortingOrder = 3;
            _harpoonBaseScale = harpoon.localScale * harpoonScale;
            harpoon.localScale = _harpoonBaseScale;

            rope.positionCount = Mathf.Max(2, ropeSegmentCount);
            rope.useWorldSpace = true;
            rope.startWidth = ropeWidth;
            rope.endWidth = ropeWidth;
            rope.numCapVertices = 6;
            rope.numCornerVertices = 2;
            ResetHarpoonInstant();
        }

        private void Update()
        {
            if (Global.IsUpgradeSelectorOpen)
            {
                UpdateRope();
                return;
            }

            if (_state == HarpoonState.Idle && !IsHarpoonSelected())
            {
                UpdateRope();
                return;
            }

            if (_state == HarpoonState.Idle && Input.GetMouseButtonDown(0))
            {
                Fire();
            }

            TickHarpoon();
            UpdateRope();
        }

        private void OnDisable()
        {
            ResetHarpoonInstant();
        }

        private void Fire()
        {
            InAir = true;
            var pivotPosition = harpoonPivot.position;
            var mousePosition = Input.mousePosition;
            var worldPoint = targetCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Mathf.Abs(targetCamera.transform.position.z)));

            worldPoint.z = pivotPosition.z;

            var direction = worldPoint - pivotPosition;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.up;
            }

            if (maxDistance > 0f)
            {
                direction = Vector3.ClampMagnitude(direction, maxDistance);
            }

            _state = HarpoonState.Flying;
            _hookedTarget = null;
            _hookedRoot = null;
            _hookedPoint = null;
            _harpoonPosition = pivotPosition;
            _targetPosition = pivotPosition + direction;
            _lastDirection = direction.normalized;
            _harpoonScaleAnimationEndTime = Time.time + 0.16f;

            harpoon.gameObject.SetActive(true);
            harpoon.DOKill();
            harpoon.localScale = _harpoonBaseScale * 0.88f;
            harpoon.DOScale(_harpoonBaseScale, 0.16f).SetEase(Ease.OutBack);
            if (harpoonCollider != null)
            {
                harpoonCollider.enabled = true;
            }
            UpdateHarpoonTransform();
        }

        private void TickHarpoon()
        {
            if (_state == HarpoonState.Idle)
            {
                return;
            }

            var previousPosition = _harpoonPosition;
            var destination = _state == HarpoonState.Flying ? _targetPosition : harpoonPivot.position;
            var speed = _state == HarpoonState.Flying ? fireSpeed * GetHarpoonSpeedModifier() : GetReturnSpeed();

            _harpoonPosition = Vector3.MoveTowards(_harpoonPosition, destination, speed * Time.deltaTime);
            var delta = _harpoonPosition - previousPosition;
            if (_state == HarpoonState.Flying && delta.sqrMagnitude > 0.000001f)
            {
                _lastDirection = delta.normalized;
            }

            UpdateHookedObject();
            UpdateHarpoonTransform();

            if (_state == HarpoonState.Flying && (_harpoonPosition - _targetPosition).sqrMagnitude < 0.000001f)
            {
                StartReturn();
            }

            if (_state == HarpoonState.Returning && (_harpoonPosition - harpoonPivot.position).sqrMagnitude < 0.000001f)
            {
                DeliverHookedTarget();
                ResetHarpoonInstant();
            }
        }

        public void TryHook(Collider2D other)
        {
            if (_state != HarpoonState.Flying || _hookedTarget != null || other == null)
            {
                return;
            }

            if (!IsInHookMask(other.gameObject.layer))
            {
                return;
            }

            var hookable = other.GetComponent<IHookable>();
            if (hookable == null)
            {
                return;
            }
            
            hookable.OnHook();
            _hookedTarget = hookable;
            _hookedRoot = hookable.RootTransform;
            _hookedPoint = hookable.HookTransform != null ? hookable.HookTransform : hookable.RootTransform;

            if (_hookedRoot == null || _hookedPoint == null)
            {
                _hookedTarget = null;
                _hookedRoot = null;
                _hookedPoint = null;
                return;
            }
            
            _hookOffset = _hookedPoint.position - _hookedRoot.position;
            _harpoonPosition = _hookedPoint.position;
            StartReturn();
        }

        private void StartReturn()
        {
            var pfb = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            Instantiate(pfb, _harpoonPosition, Quaternion.identity);
            
            _state = HarpoonState.Returning;
            if (harpoonCollider != null)
            {
                harpoonCollider.enabled = false;
            }
        }

        private void UpdateHookedObject()
        {
            if (_hookedRoot == null)
            {
                return;
            }

            _hookedRoot.position = _harpoonPosition - _hookOffset;
        }

        private float GetReturnSpeed()
        {
            var speedModifier = GetHarpoonSpeedModifier();
            if (_hookedTarget == null)
            {
                return returnSpeed * speedModifier;
            }

            return Mathf.Max(
                minReturnSpeed * speedModifier,
                returnSpeed * speedModifier / (1f + _hookedTarget.Weight * weightFactor));
        }

        private void UpdateHarpoonTransform()
        {
            var angle = Mathf.Atan2(_lastDirection.y, _lastDirection.x) * Mathf.Rad2Deg - 90f;
            UpdateHarpoonScale();
            if (harpoonBody != null)
            {
                harpoonBody.position = _harpoonPosition;
                harpoonBody.rotation = angle;
            }
            else
            {
                harpoon.position = _harpoonPosition;
                harpoon.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void UpdateRope()
        {
            if (rope == null)
            {
                return;
            }

            var visible = _state != HarpoonState.Idle;
            rope.enabled = visible;
            if (!visible)
            {
                return;
            }

            var startPosition = harpoonPivot.position;
            var endPosition = _harpoonPosition;
            var line = endPosition - startPosition;
            var distance = line.magnitude;
            var direction = distance > 0.0001f ? line / distance : Vector3.up;
            var normal = new Vector3(-direction.y, direction.x, 0f);
            if (normal.sqrMagnitude <= 0.0001f)
            {
                normal = Vector3.right;
            }

            var targetTension = GetTargetRopeTension(distance);
            _ropeTensionBlend = Mathf.Lerp(
                _ropeTensionBlend,
                targetTension,
                1f - Mathf.Exp(-ropeTensionLerpSpeed * Time.deltaTime));

            var segmentCount = Mathf.Max(2, ropeSegmentCount);
            if (rope.positionCount != segmentCount)
            {
                rope.positionCount = segmentCount;
            }

            var widthMultiplier = Mathf.Lerp(1f, returnRopeWidthMultiplier, _ropeTensionBlend);
            rope.startWidth = ropeWidth * widthMultiplier;
            rope.endWidth = ropeWidth * Mathf.Lerp(0.92f, returnRopeWidthMultiplier, _ropeTensionBlend);

            var travelFactor = Mathf.Clamp01(distance / Mathf.Max(0.001f, maxDistance <= 0f ? distance : maxDistance));
            var slackAmount = distance * ropeSlack * (1f - _ropeTensionBlend) * Mathf.Lerp(0.35f, 1f, travelFactor);
            var waveAmount = ropeWaveAmplitude * distance * (1f - _ropeTensionBlend) * Mathf.Lerp(0.25f, 1f, travelFactor);
            var sagDirection = Vector3.Lerp(normal, Vector3.down, ropeGravityBlend).normalized;
            var time = Time.time * ropeWaveTravelSpeed;

            for (var i = 0; i < segmentCount; i++)
            {
                var t = segmentCount == 1 ? 0f : i / (segmentCount - 1f);
                var point = Vector3.Lerp(startPosition, endPosition, t);

                if (i != 0 && i != segmentCount - 1)
                {
                    var arc = Mathf.Sin(t * Mathf.PI);
                    var sagOffset = sagDirection * (arc * arc * slackAmount);
                    var wavePhase = time - t * ropeWaveFrequency * Mathf.PI * 2f;
                    var waveOffset = normal * (Mathf.Sin(wavePhase) * waveAmount * arc);
                    point += sagOffset + waveOffset;
                }

                rope.SetPosition(i, point);
            }
        }

        private void ResetHarpoonInstant()
        {
            InAir = false;
            _state = HarpoonState.Idle;
            _ropeTensionBlend = 0f;
             
            _hookedTarget = null;
            _hookedRoot = null;
            _hookedPoint = null;

            if (harpoonPivot == null)
            {
                return;
            }

            _harpoonPosition = harpoonPivot.position;
            _targetPosition = _harpoonPosition;

            if (harpoon != null)
            {
                harpoon.DOKill();
                harpoon.localScale = _harpoonBaseScale;
                if (harpoonCollider != null)
                {
                    harpoonCollider.enabled = false;
                }
                if (harpoonBody != null)
                {
                    harpoonBody.position = _harpoonPosition;
                    harpoonBody.rotation = 0f;
                }
                else
                {
                    harpoon.position = _harpoonPosition;
                    harpoon.rotation = Quaternion.identity;
                }
                harpoon.gameObject.SetActive(false);
            }

            if (rope != null)
            {
                rope.enabled = false;
            }
        }

        private void DeliverHookedTarget()
        {
            if (_hookedTarget == null)
            {
                return;
            }

            _hookedTarget.OnObtain();
            submarineMovement.AddFuel(_hookedTarget.FuelAmount);


            /*if (_hookedRoot != null)
            {
                _hookedRoot.gameObject.SetActive(false);
            }*/
        }

        private bool IsInHookMask(int layer)
        {
            return (hookMask.value & (1 << layer)) != 0;
        }

        private bool IsHarpoonSelected()
        {
            return GlobalSpace.Global.ToolController == null || GlobalSpace.Global.ToolController.IsToolActive(ToolType.Harpoon);
        }

        private float GetHarpoonSpeedModifier()
        {
            if (Global.GameProgress == null)
            {
                return 1f;
            }

            return Mathf.Max(0.1f, Global.GameProgress.PlayerState.harpoonSpeedModifier);
        }

        private float GetTargetRopeTension(float distance)
        {
            if (_state == HarpoonState.Returning)
            {
                return 1f;
            }

            if (_state != HarpoonState.Flying)
            {
                return 0f;
            }

            var normalizedDistance = Mathf.Clamp01(distance / Mathf.Max(0.001f, maxDistance <= 0f ? distance : maxDistance));
            return Mathf.Lerp(0.18f, 0.48f, normalizedDistance);
        }

        private void UpdateHarpoonScale()
        {
            if (harpoon == null)
            {
                return;
            }

            if (Time.time < _harpoonScaleAnimationEndTime)
            {
                return;
            }

            var targetStretch = Mathf.Lerp(harpoonFlightStretch, harpoonReturnStretch, _ropeTensionBlend);
            var pulse = _state == HarpoonState.Flying
                ? Mathf.Sin(Time.time * 18f) * 0.015f * (1f - _ropeTensionBlend)
                : Mathf.Sin(Time.time * 26f) * 0.01f * _ropeTensionBlend;
            var stretch = 1f + targetStretch + pulse;
            var squeeze = 1f / Mathf.Sqrt(Mathf.Max(0.01f, stretch));
            var targetScale = new Vector3(
                _harpoonBaseScale.x * squeeze,
                _harpoonBaseScale.y * stretch,
                _harpoonBaseScale.z);

            harpoon.localScale = Vector3.Lerp(
                harpoon.localScale,
                targetScale,
                1f - Mathf.Exp(-harpoonStretchLerpSpeed * Time.deltaTime));
        }
    }
}
