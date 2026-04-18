using UnityEngine;

namespace Core.Submarine
{
    public class HarpoonCollisionRelay : MonoBehaviour
    {
        [SerializeField] private SubmarineHarpoonController owner;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == null)
            {
                return;
            }

            owner.TryHook(other);
        }
    }
}
