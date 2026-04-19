using System.Collections.Generic;
using Core.Submarine;
using GlobalSpace;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.SignalObjects
{
    public class HiddenSignalSpawnController : MonoBehaviour
    {
        private const string HiddenMineResourcePath = "CMS/HiddenObjects/HiddenMine";
        private const string HiddenUpgradeResourcePath = "CMS/HiddenObjects/HiddenUpgrade";

        [SerializeField] private SubmarineMovementController submarineMovement;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private GameObject hiddenMinePrefab;
        [SerializeField] private int hiddenMineMilestone;
        [SerializeField] private GameObject hiddenUpgradePrefab;
        [SerializeField] private int hiddenUpgradeMilestone;
        [SerializeField] private Vector2 spawnIntervalRange = new(3f, 5f);
        [SerializeField] private float upgradeChance = 0.25f;
        [SerializeField] private float spawnAheadOffset = 12f;
        [SerializeField] private float spawnAheadJitter = 2f;
        [SerializeField] private Vector2 spawnHorizontalRange = new(-5f, 5f);
        [SerializeField] private float minimumForwardOffset = 5f;
        [SerializeField] private float despawnBehindDistance = 16f;
        [SerializeField] private float despawnHorizontalDistance = 18f;
        [SerializeField] private int maxSpawnedSignals = 16;
        [SerializeField] private Color spawnGizmoColor = new(1f, 0.85f, 0.35f, 0.9f);

        private readonly List<GameObject> _spawnedSignals = new();
        private float _spawnTimer;

        private void Awake()
        {
            Global.HiddenSignalSpawnController = this;
            ResolveSubmarine();
            LoadDefaultPrefabs();
            ResetSpawnTimer();
        }

        private void Update()
        {
            ResolveSubmarine();
            LoadDefaultPrefabs();
            if (submarineMovement == null)
            {
                return;
            }

            TickSpawn();
            CleanupSpawnedSignals();
        }

        private void TickSpawn()
        {
            var currentMilestone = submarineMovement.CurrentMilestoneIndex;
            if (!HasAvailablePrefabs(currentMilestone))
            {
                _spawnTimer = 0f;
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f)
            {
                return;
            }

            if (_spawnedSignals.Count < maxSpawnedSignals)
            {
                SpawnSignal(currentMilestone);
            }

            ResetSpawnTimer();
        }

        private void SpawnSignal(int currentMilestone)
        {
            var prefab = GetRandomPrefab(currentMilestone);
            if (prefab == null)
            {
                return;
            }

            var spawnPosition = GetSpawnPosition();
            var instance = Instantiate(prefab, spawnPosition, Quaternion.identity, spawnParent);
            _spawnedSignals.Add(instance);
        }

        private GameObject GetRandomPrefab(int currentMilestone)
        {
            var isMineAvailable = IsPrefabAvailable(hiddenMinePrefab, hiddenMineMilestone, currentMilestone);
            var isUpgradeAvailable = IsPrefabAvailable(hiddenUpgradePrefab, hiddenUpgradeMilestone, currentMilestone);
            if (!isMineAvailable && !isUpgradeAvailable)
            {
                return null;
            }

            if (!isMineAvailable)
            {
                return hiddenUpgradePrefab;
            }

            if (!isUpgradeAvailable)
            {
                return hiddenMinePrefab;
            }

            return Random.value <= Mathf.Clamp01(upgradeChance) ? hiddenUpgradePrefab : hiddenMinePrefab;
        }

        private static bool IsPrefabAvailable(GameObject prefab, int unlockMilestone, int currentMilestone)
        {
            return prefab != null && currentMilestone >= Mathf.Max(0, unlockMilestone);
        }

        private bool HasAvailablePrefabs(int currentMilestone)
        {
            return IsPrefabAvailable(hiddenMinePrefab, hiddenMineMilestone, currentMilestone)
                   || IsPrefabAvailable(hiddenUpgradePrefab, hiddenUpgradeMilestone, currentMilestone);
        }

        private Vector3 GetSpawnPosition()
        {
            var submarinePosition = submarineMovement.transform.position;
            var spawnX = submarinePosition.x + Random.Range(spawnHorizontalRange.x, spawnHorizontalRange.y);
            var spawnY = submarinePosition.y + spawnAheadOffset + Random.Range(-spawnAheadJitter, spawnAheadJitter);
            spawnY = Mathf.Max(spawnY, submarinePosition.y + minimumForwardOffset);
            return new Vector3(spawnX, spawnY, submarinePosition.z);
        }

        private void CleanupSpawnedSignals()
        {
            var submarinePosition = submarineMovement.transform.position;

            for (var i = _spawnedSignals.Count - 1; i >= 0; i--)
            {
                var signal = _spawnedSignals[i];
                if (signal == null)
                {
                    _spawnedSignals.RemoveAt(i);
                    continue;
                }

                var signalPosition = signal.transform.position;
                var behindSubmarine = signalPosition.y < submarinePosition.y - despawnBehindDistance;
                var tooFarSideways = Mathf.Abs(signalPosition.x - submarinePosition.x) > despawnHorizontalDistance;

                if (!behindSubmarine && !tooFarSideways)
                {
                    continue;
                }

                _spawnedSignals.RemoveAt(i);
                Destroy(signal);
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

        private void LoadDefaultPrefabs()
        {
            hiddenMinePrefab ??= Resources.Load<GameObject>(HiddenMineResourcePath);
            hiddenUpgradePrefab ??= Resources.Load<GameObject>(HiddenUpgradeResourcePath);
        }

        private void ResetSpawnTimer()
        {
            var minDelay = Mathf.Max(0.1f, Mathf.Min(spawnIntervalRange.x, spawnIntervalRange.y));
            var maxDelay = Mathf.Max(minDelay, Mathf.Max(spawnIntervalRange.x, spawnIntervalRange.y));
            _spawnTimer = Random.Range(minDelay, maxDelay);
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
