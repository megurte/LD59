using UnityEngine;

namespace Core.Submarine
{
    public interface IHookable
    {
        float Weight { get; }
        float FuelAmount { get; }
        Transform RootTransform { get; }
        Transform HookTransform { get; }
        void OnHook();
    }
}
