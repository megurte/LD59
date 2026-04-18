using UnityEngine;

namespace Core.Fish
{
    public class FishMovementController : MonoBehaviour
    {
        [SerializeField] private Vector2 squareSize = new(4f, 4f);
        [SerializeField] private float moveSpeed = 1.6f;
        [SerializeField] private float turnSmoothTime = 0.18f;
        [SerializeField] private float maxTurnSpeed = 540f;
        [SerializeField] private float cornerLookAheadDistance = 0.9f;
        [SerializeField] private bool clockwise;

        private readonly Vector3[] _points = new Vector3[4];

        private int _targetIndex = 1;
        private float _turnVelocity;
        private float _turnAmount;
        private Vector3 _velocity;

        public float SpeedNormalized => moveSpeed <= 0f ? 0f : Mathf.Clamp01(_velocity.magnitude / moveSpeed);
        public float TurnAmount => _turnAmount;

        private void Awake()
        {
            BuildRoute();
        }

        private void Update()
        {
            if (moveSpeed <= 0f)
            {
                _velocity = Vector3.zero;
                _turnAmount = Mathf.MoveTowards(_turnAmount, 0f, Time.deltaTime * 6f);
                return;
            }

            var currentPosition = transform.position;
            var currentTarget = _points[_targetIndex];
            var nextPosition = Vector3.MoveTowards(currentPosition, currentTarget, moveSpeed * Time.deltaTime);

            transform.position = nextPosition;
            _velocity = (nextPosition - currentPosition) / Mathf.Max(Time.deltaTime, 0.0001f);

            if ((currentTarget - nextPosition).sqrMagnitude <= 0.0001f)
            {
                _targetIndex = (_targetIndex + 1) % _points.Length;
                currentTarget = _points[_targetIndex];
            }

            RotateTowards(nextPosition, currentTarget);
        }

        private void BuildRoute()
        {
            var start = transform.position;
            var width = Mathf.Max(0.01f, squareSize.x);
            var height = Mathf.Max(0.01f, squareSize.y);

            _points[0] = start;

            if (clockwise)
            {
                _points[1] = start + Vector3.up * height;
                _points[2] = start + new Vector3(width, height, 0f);
                _points[3] = start + Vector3.right * width;
                return;
            }

            _points[1] = start + Vector3.right * width;
            _points[2] = start + new Vector3(width, height, 0f);
            _points[3] = start + Vector3.up * height;
        }

        private void RotateTowards(Vector3 position, Vector3 target)
        {
            var direction = GetFacingDirection(position, target);
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

        private Vector3 GetFacingDirection(Vector3 position, Vector3 target)
        {
            var toTarget = target - position;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return transform.up;
            }

            var targetDistance = toTarget.magnitude;
            var targetDirection = toTarget / targetDistance;
            if (cornerLookAheadDistance <= 0f)
            {
                return targetDirection;
            }

            var nextDirection = (_points[(_targetIndex + 1) % _points.Length] - target).normalized;
            if (nextDirection.sqrMagnitude <= 0.0001f)
            {
                return targetDirection;
            }

            var blend = 1f - Mathf.Clamp01(targetDistance / cornerLookAheadDistance);
            return Vector3.Lerp(targetDirection, nextDirection, blend).normalized;
        }
    }
}
