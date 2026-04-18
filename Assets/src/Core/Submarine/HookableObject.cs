using System.Collections.Generic;
using UnityEngine;

namespace Core.Submarine
{
    public class HookableObject : MonoBehaviour, IHookable
    {
        [SerializeField] private float weight = 1f;
        [SerializeField] private float fuelAmount = 10f;
        [SerializeField] private Transform hookTransform;
        [SerializeField] private Collider2D targetCollider;

        private static readonly Dictionary<Collider2D, HookableObject> Registry = new();

        public float Weight => weight;
        public float FuelAmount => fuelAmount;
        public Transform RootTransform => transform;
        public Transform HookTransform => hookTransform != null ? hookTransform : transform;

        private void OnEnable()
        {
            if (targetCollider == null)
            {
                return;
            }

            Registry[targetCollider] = this;
        }

        private void OnDisable()
        {
            if (targetCollider == null)
            {
                return;
            }

            if (Registry.TryGetValue(targetCollider, out var current) && current == this)
            {
                Registry.Remove(targetCollider);
            }
        }

        public static bool TryGet(Collider2D collider, out IHookable hookable)
        {
            if (collider != null && Registry.TryGetValue(collider, out var hookableObject))
            {
                hookable = hookableObject;
                return true;
            }

            hookable = null;
            return false;
        }
    }
}
