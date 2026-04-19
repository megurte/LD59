using System.Collections.Generic;
using Constants;
using Core.Fish;
using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class CannonProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private float mineExplosionRadius = 1.2f;

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
                if (ShouldExplodeOnHit())
                {
                    Explode();
                    return;
                }

                damagable.OnTakeDamage(1);
                Destroy(gameObject);
            }
        }

        private void Explode()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, GetExplosionRadius());
            var damagedTargets = new HashSet<IDamageable>();

            foreach (var targetCollider in colliders)
            {
                if (!targetCollider.TryGetComponent<IDamageable>(out var damageable))
                {
                    continue;
                }

                if (!damagedTargets.Add(damageable))
                {
                    continue;
                }

                damageable.OnTakeDamage(1);
            }

            var burst = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            if (burst != null)
            {
                Instantiate(burst, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        private float GetExplosionRadius()
        {
            if (Global.GameProgress == null)
            {
                return mineExplosionRadius;
            }

            return mineExplosionRadius * Mathf.Max(0.1f, Global.GameProgress.PlayerState.projectileExplosionRadiusModifier);
        }

        private bool ShouldExplodeOnHit()
        {
            return Global.GameProgress != null && Global.GameProgress.PlayerState.mineProjectiles;
        }
    }
}
