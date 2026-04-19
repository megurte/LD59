using System;
using System.Collections.Generic;
using System.Linq;
using Core.Upgrade.Upgrades;
using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade
{
    public class UpgradeDropService
    {
        private readonly List<UpgradeDropEntry> _dropMap = new()
        {
            new UpgradeDropEntry(() => new FuelRefillUpgrade(), 1.4f),
            new UpgradeDropEntry(() => new FuelRefillPlusUpgrade(), 0.6f),
            new UpgradeDropEntry(() => new HarpoonSpeedUpgrade(), 1f),
            new UpgradeDropEntry(() => new CannonCooldownUpgrade(), 1f),
            new UpgradeDropEntry(() => new ExplosionProjectileUpgrade(), 6f),
            new UpgradeDropEntry(() => new ProjectileRadiusUpgrade(), 0.8f),
            new UpgradeDropEntry(() => new SpeedBoostTimeUpgrade(), 1f),
            new UpgradeDropEntry(() => new SubmarineSpeedBoostUpgrade(), 6f)
        };

        public List<IUpgrade> GetRandomUpgrades(int count)
        {
            var result = new List<IUpgrade>(count);
            if (count <= 0)
            {
                return result;
            }

            var availableEntries = GetAvailableEntries();
            if (availableEntries.Count < count)
            {
                Debug.LogWarning($"UpgradeDropService has only {availableEntries.Count} available upgrades for requested {count}.");
            }

            var targetCount = Mathf.Min(count, availableEntries.Count);
            for (var i = 0; i < targetCount; i++)
            {
                var selectedEntry = PickRandomEntry(availableEntries);
                if (selectedEntry == null)
                {
                    break;
                }

                result.Add(selectedEntry.Create());
                availableEntries.Remove(selectedEntry);
            }

            return result;
        }

        private List<UpgradeDropEntry> GetAvailableEntries()
        {
            var result = new List<UpgradeDropEntry>(_dropMap.Count);

            foreach (var entry in _dropMap)
            {
                var upgrade = entry.Create();
                if (upgrade.Condition == null
                    || upgrade.Condition.IsSatisfied())
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        private UpgradeDropEntry PickRandomEntry(List<UpgradeDropEntry> entries)
        {
            var totalWeight = entries.Sum(entry => Mathf.Max(0f, entry.Weight));

            if (totalWeight <= 0f)
            {
                return null;
            }

            var randomWeight = UnityEngine.Random.Range(0f, totalWeight);
            foreach (var entry in entries)
            {
                randomWeight -= Mathf.Max(0f, entry.Weight);
                if (randomWeight <= 0f)
                {
                    return entry;
                }
            }

            return entries[^1];
        }

        private class UpgradeDropEntry
        {
            private readonly Func<IUpgrade> _factory;

            public float Weight { get; }

            public UpgradeDropEntry(Func<IUpgrade> factory, float weight)
            {
                _factory = factory;
                Weight = weight;
            }

            public IUpgrade Create()
            {
                return _factory.Invoke();
            }
        }
    }
}
