using Core.Submarine;
using UnityEngine;

namespace Core.Fish
{
    public class FishState : MonoBehaviour, IHookable
    {
        [SerializeField] private float weight = 1;
        [SerializeField] private float fuel = 8;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform hookTransform;

        public float Weight => weight;
        public float FuelAmount => fuel;
        public Transform RootTransform => rootTransform;
        public Transform HookTransform => hookTransform;
    }
}