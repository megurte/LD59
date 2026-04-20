using Core.Submarine;
using GlobalSpace;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Fish
{
    public class FishSphereMovementController : MonoBehaviour
    {
        [SerializeField] private SubmarineMovementController submarineMovement;
        [SerializeField] private Transform rotatingVisual;
        [SerializeField] private float moveSpeed = 6.2f;
        [SerializeField] private Vector2 rotationSpeedRange = new(220f, 420f);
        [SerializeField] private float aimJitterRadius = 0.65f;
        [SerializeField] private bool randomizeSpinDirection = true;

        private Vector3 _moveDirection = Vector3.down;
        private float _rotationSpeed;
        private bool _directionCaptured;
        private bool _isMovementStopped;

        private void Awake()
        {
            rotatingVisual ??= transform;

            var minRotationSpeed = Mathf.Min(rotationSpeedRange.x, rotationSpeedRange.y);
            var maxRotationSpeed = Mathf.Max(minRotationSpeed, rotationSpeedRange.y);
            _rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
            if (randomizeSpinDirection && Random.value < 0.5f)
            {
                _rotationSpeed = -_rotationSpeed;
            }

            CaptureMoveDirection(true);
        }

        private void Update()
        {
            if (_isMovementStopped)
            {
                return;
            }

            if (!_directionCaptured)
            {
                CaptureMoveDirection();
            }

            transform.position += _moveDirection * moveSpeed * Time.deltaTime;

            if (rotatingVisual != null)
            {
                rotatingVisual.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
            }
        }

        public void BindSubmarine(SubmarineMovementController movementController)
        {
            submarineMovement = movementController;
            CaptureMoveDirection(true);
        }

        public void StopMovement()
        {
            _isMovementStopped = true;
        }

        private void CaptureMoveDirection(bool forceRecapture = false)
        {
            if (_directionCaptured && !forceRecapture)
            {
                return;
            }

            submarineMovement ??= Global.SubmarineMovement;

            // Sphere captures the direction once at spawn time and then flies straight.
            var targetPosition = submarineMovement != null
                ? submarineMovement.transform.position + (Vector3)(Random.insideUnitCircle * aimJitterRadius)
                : transform.position + Vector3.down;

            var direction = targetPosition - transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.down;
            }

            _moveDirection = direction.normalized;
            _directionCaptured = true;
        }
    }
}
