using System;
using System.Collections.Generic;
using Core.Submarine;
using GlobalSpace;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Fish
{
    public class FishSpawnController : MonoBehaviour
    {
        [Serializable]
        private class FishSpawnRule
        {
            public GameObject prefab;
            public Vector2 intervalRange = new(3f, 5f);
            [Range(0f, 1f)] public float schoolChance = 0.35f;
            public Vector2Int schoolSizeRange = new(3, 5);
            public Vector2 schoolSpacingRange = new(0.7f, 1.2f);
        }

        [SerializeField] private SubmarineMovementController submarineMovement;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private List<FishSpawnRule> spawnRules = new();
        [SerializeField] private float spawnAheadOffset = 10f;
        [SerializeField] private float spawnAheadJitter = 2f;
        [SerializeField] private Vector2 spawnHorizontalRange = new(-6f, 6f);
        [SerializeField] private float minimumForwardOffset = 4f;
        [SerializeField] private float despawnBehindDistance = 14f;
        [SerializeField] private float despawnHorizontalDistance = 18f;
        [SerializeField] private int maxSpawnedFish = 48;
        [SerializeField] private Color spawnGizmoColor = new(0.3f, 1f, 0.75f, 0.9f);

        private readonly List<float> _spawnTimers = new();
        private readonly List<GameObject> _spawnedFish = new();

        private void Awake()
        {
            ResolveSubmarine();
            SyncRuleTimers();
        }

        private void Update()
        {
            ResolveSubmarine();
            if (submarineMovement == null)
            {
                return;
            }

            SyncRuleTimers();
            TickSpawnRules();
            CleanupSpawnedFish();
        }

        private void TickSpawnRules()
        {
            for (var i = 0; i < spawnRules.Count; i++)
            {
                var rule = spawnRules[i];
                if (rule == null || rule.prefab == null)
                {
                    continue;
                }

                _spawnTimers[i] -= Time.deltaTime;
                if (_spawnTimers[i] > 0f)
                {
                    continue;
                }

                if (_spawnedFish.Count < maxSpawnedFish)
                {
                    SpawnRule(rule);
                }

                _spawnTimers[i] = GetNextDelay(rule);
            }
        }

        private void SpawnRule(FishSpawnRule rule)
        {
            var schoolSize = ShouldSpawnSchool(rule)
                ? Random.Range(Mathf.Max(2, rule.schoolSizeRange.x), Mathf.Max(2, rule.schoolSizeRange.y) + 1)
                : 1;

            var anchor = GetSpawnAnchor();
            var spacing = Mathf.Max(0.1f, Random.Range(rule.schoolSpacingRange.x, rule.schoolSpacingRange.y));

            for (var i = 0; i < schoolSize; i++)
            {
                var spawnPosition = anchor + GetSchoolOffset(i, schoolSize, spacing);
                spawnPosition.y = Mathf.Max(spawnPosition.y, submarineMovement.transform.position.y + minimumForwardOffset);

                var fishInstance = Instantiate(rule.prefab, spawnPosition, Quaternion.identity, spawnParent);
                var movement = fishInstance.GetComponent<FishMovementController>();
                if (movement != null)
                {
                    movement.BindSubmarine(submarineMovement);
                }

                _spawnedFish.Add(fishInstance);
            }
        }

        private Vector3 GetSpawnAnchor()
        {
            var submarinePosition = submarineMovement.transform.position;
            var spawnX = submarinePosition.x + Random.Range(spawnHorizontalRange.x, spawnHorizontalRange.y);
            var spawnY = submarinePosition.y + spawnAheadOffset + Random.Range(-spawnAheadJitter, spawnAheadJitter);
            spawnY = Mathf.Max(spawnY, submarinePosition.y + minimumForwardOffset);
            return new Vector3(spawnX, spawnY, submarinePosition.z);
        }

        private static Vector3 GetSchoolOffset(int index, int schoolSize, float spacing)
        {
            if (schoolSize <= 1 || index == 0)
            {
                return Vector3.zero;
            }

            var row = (index - 1) / 2 + 1;
            var side = (index & 1) == 0 ? 1f : -1f;
            var horizontalOffset = side * spacing * row;
            var verticalOffset = -spacing * 0.45f * row + Random.Range(-spacing * 0.15f, spacing * 0.15f);
            return new Vector3(horizontalOffset, verticalOffset, 0f);
        }

        private void CleanupSpawnedFish()
        {
            var submarinePosition = submarineMovement.transform.position;

            for (var i = _spawnedFish.Count - 1; i >= 0; i--)
            {
                var fish = _spawnedFish[i];
                if (fish == null)
                {
                    _spawnedFish.RemoveAt(i);
                    continue;
                }

                var fishPosition = fish.transform.position;
                var behindSubmarine = fishPosition.y < submarinePosition.y - despawnBehindDistance;
                var tooFarSideways = Mathf.Abs(fishPosition.x - submarinePosition.x) > despawnHorizontalDistance;

                if (!behindSubmarine && !tooFarSideways)
                {
                    continue;
                }

                _spawnedFish.RemoveAt(i);
                Destroy(fish);
            }
        }

        private void ResolveSubmarine()
        {
            if (submarineMovement != null)
            {
                return;
            }

            submarineMovement = Global.SubmarineMovement;
            if (submarineMovement == null)
            {
                submarineMovement = FindFirstObjectByType<SubmarineMovementController>();
            }
        }

        private void SyncRuleTimers()
        {
            while (_spawnTimers.Count < spawnRules.Count)
            {
                _spawnTimers.Add(0f);
            }

            while (_spawnTimers.Count > spawnRules.Count)
            {
                _spawnTimers.RemoveAt(_spawnTimers.Count - 1);
            }
        }

        private static float GetNextDelay(FishSpawnRule rule)
        {
            var minDelay = Mathf.Max(0.1f, Mathf.Min(rule.intervalRange.x, rule.intervalRange.y));
            var maxDelay = Mathf.Max(minDelay, Mathf.Max(rule.intervalRange.x, rule.intervalRange.y));
            return Random.Range(minDelay, maxDelay);
        }

        private static bool ShouldSpawnSchool(FishSpawnRule rule)
        {
            return Random.value <= Mathf.Clamp01(rule.schoolChance);
        }

        private void OnDrawGizmosSelected()
        {
            var targetSubmarine = submarineMovement != null ? submarineMovement : Global.SubmarineMovement;
            if (targetSubmarine == null)
            {
                return;
            }

            var submarinePosition = targetSubmarine.transform.position;
            var center = new Vector3(
                submarinePosition.x,
                submarinePosition.y + Mathf.Max(spawnAheadOffset, minimumForwardOffset) + spawnAheadJitter * 0.5f,
                submarinePosition.z);
            var size = new Vector3(
                Mathf.Abs(spawnHorizontalRange.y - spawnHorizontalRange.x),
                Mathf.Max(0.5f, spawnAheadJitter * 2f),
                0.1f);

            Gizmos.color = spawnGizmoColor;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
