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
        [SerializeField] private float harpoonScale = 1f;
        [SerializeField] private LayerMask hookMask = ~0;

        public bool InAir {get; private set;}
        
        private Vector3 _harpoonBaseScale;
        private Vector3 _harpoonPosition;
        private Vector3 _targetPosition;
        private Vector3 _lastDirection = Vector3.up;

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

            rope.positionCount = 2;
            rope.useWorldSpace = true;
            rope.startWidth = ropeWidth;
            rope.endWidth = ropeWidth;
            rope.numCapVertices = 6;
            rope.numCornerVertices = 2;
            ResetHarpoonInstant();
        }

        private void Update()
        {
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
            var speed = _state == HarpoonState.Flying ? fireSpeed : GetReturnSpeed();

            _harpoonPosition = Vector3.MoveTowards(_harpoonPosition, destination, speed * Time.deltaTime);
            var delta = _harpoonPosition - previousPosition;
            if (delta.sqrMagnitude > 0.000001f)
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

            var hookable = other.GetComponent<IHookable>() ?? other.GetComponentInParent<IHookable>();
            if (hookable == null)
            {
                return;
            }

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
            if (_hookedTarget == null)
            {
                return returnSpeed;
            }

            return Mathf.Max(minReturnSpeed, returnSpeed / (1f + _hookedTarget.Weight * weightFactor));
        }

        private void UpdateHarpoonTransform()
        {
            var angle = Mathf.Atan2(_lastDirection.y, _lastDirection.x) * Mathf.Rad2Deg - 90f;
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

            rope.SetPosition(0, harpoonPivot.position);
            rope.SetPosition(1, _harpoonPosition);
        }

        private void ResetHarpoonInstant()
        {
            InAir = false;
            _state = HarpoonState.Idle;
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

            if (submarineMovement != null)
            {
                submarineMovement.AddFuel(_hookedTarget.FuelAmount);
            }

            if (_hookedRoot != null)
            {
                _hookedRoot.gameObject.SetActive(false);
            }
        }

        private bool IsInHookMask(int layer)
        {
            return (hookMask.value & (1 << layer)) != 0;
        }

        private bool IsHarpoonSelected()
        {
            return GlobalSpace.Global.ToolController == null || GlobalSpace.Global.ToolController.IsToolActive(ToolType.Harpoon);
        }
    }
}
