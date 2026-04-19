using Core.Fish;
using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class CannonProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private GameObject vfx;

        public void Launch(Vector2 direction, float speed)
        {
            if (body == null)
            {
                return;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.up;
            }

            body.linearVelocity = direction.normalized * speed;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IDamageable>(out var damagable))
            {
                damagable.OnTakeDamage(1);
                vfx.transform.SetParent(Global.FishSpawnController.gameObject.transform);
                vfx.transform.localScale = Vector3.one;
                var ps = vfx.GetComponent<ParticleSystem>();
                var main = ps.main;
                main.loop = false;
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                Destroy(gameObject);
            }
        }
    }
}
