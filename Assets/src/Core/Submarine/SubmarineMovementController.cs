using System.Collections.Generic;
using Constants;
using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineMovementController : MonoBehaviour
    {
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
        [SerializeField] private Vector3 moveDirection = Vector3.right;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float startFuel = 100f;
        [SerializeField] private float fuelBurnPerSecond = 1f;
        [SerializeField] private SubmarineTakeableCollector takeableCollector;
            
        private float _currentFuel;
        private float _speedBoostEndTime;
        private float _speedBoostMultiplier = 1f;
        private bool _milestonesDirty = true;
        private readonly List<MilestoneData> _milestones = new();

        public float FuelNormalized => maxFuel <= 0f ? 0f : _currentFuel / maxFuel;
        public float RouteProgressNormalized => GetRouteProgress();
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
        }
    }
}
