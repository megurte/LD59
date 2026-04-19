using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineMovementController : MonoBehaviour
    {
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private Vector3 moveDirection = Vector3.right;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float startFuel = 100f;
        [SerializeField] private float fuelBurnPerSecond = 1f;

        private float _currentFuel;
        private float _speedBoostEndTime;
        private float _speedBoostMultiplier = 1f;

        public float FuelNormalized => maxFuel <= 0f ? 0f : _currentFuel / maxFuel;
        public float RouteProgressNormalized => GetRouteProgress();

        private void Awake()
        {
            Global.SubmarineMovement = this;
            _currentFuel = Mathf.Clamp(startFuel, 0f, maxFuel);
        }

        private void Update()
        {
            if (_currentFuel <= 0f)
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
                transform.position = endPoint.position;
                return;
            }

            transform.position += direction * GetCurrentMoveSpeed() * Time.deltaTime;
            _currentFuel = Mathf.Max(0f, _currentFuel - fuelBurnPerSecond * Time.deltaTime);

            if (HasReachedEnd(direction))
            {
                transform.position = endPoint.position;
            }
        }

        public void AddFuel(float amount)
        {
            _currentFuel = Mathf.Clamp(_currentFuel + amount, 0f, maxFuel);
        }

        public void SubstructFuel(float amount)
        {
            _currentFuel = Mathf.Clamp(_currentFuel - amount, 0f, maxFuel);
        }

        public void ApplyTemporarySpeedBoost(float multiplier, float duration)
        {
            _speedBoostMultiplier = Mathf.Max(_speedBoostMultiplier, multiplier);
            _speedBoostEndTime = Mathf.Max(_speedBoostEndTime, Time.time + duration);
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
    }
}
