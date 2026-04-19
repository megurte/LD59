using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineTakeableCollector : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryTake(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryTake(other);
        }

        private void TryTake(Collider2D other)
        {
            var takeable = other.GetComponent<ITakeable>();
            takeable?.Take(Global.SubmarineMovement);
        }
    }
}