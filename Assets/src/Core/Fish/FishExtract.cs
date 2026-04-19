using Core.Submarine;
using UnityEngine;

namespace Core.Fish
{
    public class FishExtract : MonoBehaviour, ITakeable, IHookable
    {
        public void Take(SubmarineMovementController submarineMovement)
        {
            GlobalSpace.Global.SubmarineMovement.AddFuel(20);
            Destroy(gameObject);
        }

        public float Weight => 0.5f;
        public float FuelAmount => 0;
        public Transform RootTransform => transform;
        public Transform HookTransform => transform;
        public void OnHook()
        {
        }

        public void OnObtain()
        {
        }
    }
}