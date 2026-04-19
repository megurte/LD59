using System;
using Common;
using Core.Submarine;
using GlobalSpace;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Fish
{
    public class FishMovementController : MonoBehaviour
    {
        [SerializeField] private SubmarineMovementController submarineMovement;
        [SerializeField] private float cruiseSpeed = 1.6f;
        [SerializeField] private float escapeSpeed = 3.2f;
        [SerializeField] private float turnSmoothTime = 0.18f;
        [SerializeField] private float maxTurnSpeed = 540f;
        [SerializeField] private float waypointReachDistance = 0.35f;
        [SerializeField] private float reactionRadius = 2.5f;
        [SerializeField] private float escapeReleaseRadius = 3.3f;
        [SerializeField] private float escapeMinDuration = 0.8f;
        [SerializeField] private float targetRefreshInterval = 2.4f;
        [SerializeField] private float randomRetargetJitter = 0.6f;
        [SerializeField] private Vector2 approachOffsetRange = new(0.25f, 1.1f);
        [SerializeField] private Vector2 passOffsetRange = new(1.5f, 3.5f);
        [SerializeField] private Vector2 passForwardRange = new(-1.5f, 1.5f);
        [SerializeField] private Color reactionGizmoColor = new(0.2f, 0.85f, 1f, 0.9f);
        [SerializeField] private ParticleSystem bubblesBurst;
        
        private FishMoveState _state;
        private Transform _submarineTransform;
        private Vector3 _currentTarget;
        private Vector3 _escapeDirection = Vector3.up;
        private Vector3 _velocity;
        private float _turnVelocity;
        private float _turnAmount;
        private float _targetRefreshTimer;
        private float _escapeTimer;

        public float SpeedNormalized
        {
            get
            {
                var maxConfiguredSpeed = Mathf.Max(cruiseSpeed, escapeSpeed, 0.0001f);
                return Mathf.Clamp01(_velocity.magnitude / maxConfiguredSpeed);
            }
        }

        public float TurnAmount => _turnAmount;

        private enum FishMoveState
        {
            Cruise,
            Escape
        }

        private void Awake()
        {
            ChooseCruiseTarget(true);
        }

        private void Update()
        {
            if (Mathf.Max(cruiseSpeed, escapeSpeed) <= 0f)
            {
                _velocity = Vector3.zero;
                _turnAmount = Mathf.MoveTowards(_turnAmount, 0f, Time.deltaTime * 6f);
                return;
            }

            if (_state == FishMoveState.Cruise && ShouldStartEscape())
            {
                BeginEscape();
            }

            switch (_state)
            {
                case FishMoveState.Cruise:
                    TickCruise();
                    break;
                case FishMoveState.Escape:
                    TickEscape();
                    break;
            }
        }

        private void TickCruise()
        {
            _targetRefreshTimer -= Time.deltaTime;

            var currentPosition = transform.position;
            var toTarget = _currentTarget - currentPosition;
            toTarget.z = 0f;

            if (toTarget.sqrMagnitude <= waypointReachDistance * waypointReachDistance || _targetRefreshTimer <= 0f)
            {
                ChooseCruiseTarget(false);
                toTarget = _currentTarget - currentPosition;
                toTarget.z = 0f;
            }

            var direction = GetSafeDirection(toTarget, transform.up);
            Move(direction, cruiseSpeed);
        }

        private void TickEscape()
        {
            _escapeTimer += Time.deltaTime;

            if (_submarineTransform != null)
            {
                var fromSubmarine = transform.position - _submarineTransform.position;
                fromSubmarine.z = 0f;

                if (fromSubmarine.sqrMagnitude > 0.0001f)
                {
                    var preferredEscapeDirection = GetPerpendicularDirection(fromSubmarine.normalized);
                    _escapeDirection = Vector3.Lerp(_escapeDirection, preferredEscapeDirection, 1f - Mathf.Exp(-8f * Time.deltaTime)).normalized;
                }

                if (_escapeTimer >= escapeMinDuration && fromSubmarine.magnitude >= escapeReleaseRadius)
                {
                    _state = FishMoveState.Cruise;
                    ChooseCruiseTarget(true);
                }
            }

            Move(GetSafeDirection(_escapeDirection, transform.up), escapeSpeed);
        }

        private void Move(Vector3 direction, float speed)
        {
            if (speed <= 0f || direction.sqrMagnitude <= 0.0001f)
            {
                _velocity = Vector3.zero;
                _turnAmount = Mathf.MoveTowards(_turnAmount, 0f, Time.deltaTime * 6f);
                return;
            }

            direction.z = 0f;
            var currentPosition = transform.position;
            var nextPosition = currentPosition + direction.normalized * speed * Time.deltaTime;

            transform.position = nextPosition;
            _velocity = (nextPosition - currentPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            RotateTowards(direction.normalized);
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                _turnAmount = Mathf.MoveTowards(_turnAmount, 0f, Time.deltaTime * 6f);
                return;
            }

            var currentAngle = transform.eulerAngles.z;
            var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            var nextAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref _turnVelocity, turnSmoothTime, maxTurnSpeed);
            var deltaAngle = Mathf.DeltaAngle(currentAngle, nextAngle) / Mathf.Max(Time.deltaTime, 0.0001f);

            transform.rotation = Quaternion.Euler(0f, 0f, nextAngle);
            _turnAmount = maxTurnSpeed <= 0f ? 0f : Mathf.Clamp(deltaAngle / maxTurnSpeed, -1f, 1f);
        }

        private bool ShouldStartEscape()
        {
            if (Global.SubmarineMovement != null)
            {
                _submarineTransform = Global.SubmarineMovement.transform;
            }
            else if (submarineMovement != null)
            {
                _submarineTransform = submarineMovement.transform;
            }

            if (_submarineTransform == null)
            {
                return false;
            }

            var toSubmarine = _submarineTransform.position - transform.position;
            toSubmarine.z = 0f;
            return toSubmarine.sqrMagnitude <= reactionRadius * reactionRadius;
        }

        public void BeginEscape()
        {
            var fishState = GetComponent<FishState>();
            
            if (fishState.IsHooked) return; 
            
            _state = FishMoveState.Escape;
            _escapeTimer = 0f;
            var inst = Global.EffectFactory.Create(bubblesBurst);
            inst.transform.position = transform.position;
            GameAudio.PlayBubbleSpawn(0.14f, 0.98f, 1.08f);

            var fromSubmarine = transform.position - (_submarineTransform != null ? _submarineTransform.position : transform.position);
            fromSubmarine.z = 0f;

            var awayDirection = GetSafeDirection(fromSubmarine, transform.up);
            _escapeDirection = GetPerpendicularDirection(awayDirection);
        }

        private void ChooseCruiseTarget(bool immediateRefresh)
        {
            _state = FishMoveState.Cruise;
            _targetRefreshTimer = targetRefreshInterval + Random.Range(-randomRetargetJitter, randomRetargetJitter);
            _targetRefreshTimer = Mathf.Max(0.25f, _targetRefreshTimer);

            if (_submarineTransform == null)
            {
                _currentTarget = transform.position + GetRandomDirection() * Random.Range(2f, 5f);
                return;
            }

            var submarinePosition = _submarineTransform.position;
            var toSubmarine = submarinePosition - transform.position;
            toSubmarine.z = 0f;

            var forwardToSubmarine = GetSafeDirection(toSubmarine, GetRandomDirection());
            var side = Random.value < 0.5f ? -1f : 1f;
            var tangent = new Vector3(-forwardToSubmarine.y, forwardToSubmarine.x, 0f) * side;

            if (Random.value < 0.5f)
            {
                var approachOffset = Random.Range(approachOffsetRange.x, approachOffsetRange.y);
                _currentTarget = submarinePosition - forwardToSubmarine * approachOffset;
            }
            else
            {
                var lateralOffset = Random.Range(passOffsetRange.x, passOffsetRange.y);
                var forwardOffset = Random.Range(passForwardRange.x, passForwardRange.y);
                _currentTarget = submarinePosition + tangent * lateralOffset + forwardToSubmarine * forwardOffset;
            }

            if (!immediateRefresh)
            {
                _currentTarget += (Vector3)(Random.insideUnitCircle * 0.35f);
            }
        }

        private Vector3 GetPerpendicularDirection(Vector3 awayDirection)
        {
            var left = new Vector3(-awayDirection.y, awayDirection.x, 0f);
            var right = -left;
            var currentForward = GetSafeDirection(_velocity, transform.up);

            if (Vector3.Dot(currentForward, left) >= Vector3.Dot(currentForward, right))
            {
                return left.normalized;
            }

            return right.normalized;
        }

        private Vector3 GetRandomDirection()
        {
            var direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.up;
            }

            return new Vector3(direction.x, direction.y, 0f);
        }

        private static Vector3 GetSafeDirection(Vector3 preferred, Vector3 fallback)
        {
            preferred.z = 0f;
            if (preferred.sqrMagnitude > 0.0001f)
            {
                return preferred.normalized;
            }

            fallback.z = 0f;
            if (fallback.sqrMagnitude > 0.0001f)
            {
                return fallback.normalized;
            }

            return Vector3.up;
        }

        public void BindSubmarine(SubmarineMovementController movementController)
        {
            submarineMovement = movementController;
            _submarineTransform = submarineMovement != null ? submarineMovement.transform : null;
        }

        private void OnDrawGizmosSelected()
        {
            //Gizmos.color = reactionGizmoColor;
            //Gizmos.DrawWireSphere(Global.SubmarineMovement.transform.position, reactionRadius);
        }
    }
}
