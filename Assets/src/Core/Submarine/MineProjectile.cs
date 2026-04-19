using System.Collections.Generic;
using Common;
using Constants;
using Core.Fish;
using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class MineProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private float explosionRadius = 1.2f;
        [SerializeField] private int minBubbleBurstCount = 10;
        [SerializeField] private float bubbleBurstDensity = 7f;
        [SerializeField] private float bubbleInnerRingFactor = 0.55f;
        [SerializeField] private float bubbleScale = 0.8f;
        [SerializeField] private float explosionShakeStrength = 0.2f;
        [SerializeField] private float explosionShakeDuration = 0.22f;
        [SerializeField] private float explosionShakeFrequency = 27f;

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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<IDamageable>(out _))
            {
                return;
            }

            Explode();
        }

        private void Explode()
        {
            GameAudio.PlayExplosion(0.62f, 0.93f, 1.01f);
            var radius = GetExplosionRadius();
            var colliders = Physics2D.OverlapCircleAll(transform.position, radius);
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

            SpawnExplosionBubbles(radius);
            Global.SubmarineCameraController?.PlayImpulseShake(
                explosionShakeStrength,
                explosionShakeDuration,
                explosionShakeFrequency);
            Destroy(gameObject);
        }

        private void SpawnExplosionBubbles(float radius)
        {
            var bubbleBurst = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            if (bubbleBurst == null)
            {
                return;
            }

            GameAudio.PlayBubbleSpawn(0.22f, 0.9f, 1.02f);
            var ringBurstCount = Mathf.Max(minBubbleBurstCount, Mathf.CeilToInt(radius * bubbleBurstDensity));
            for (var i = 0; i < ringBurstCount; i++)
            {
                var angle = Mathf.PI * 2f * i / ringBurstCount;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                SpawnBubbleBurst(bubbleBurst, direction * radius);
                SpawnBubbleBurst(bubbleBurst, direction * radius * bubbleInnerRingFactor);
            }

            SpawnBubbleBurst(bubbleBurst, Vector3.zero);
        }

        private void SpawnBubbleBurst(ParticleSystem bubbleBurst, Vector3 offset)
        {
            var burst = Instantiate(bubbleBurst, transform.position + offset, Quaternion.identity);
            burst.transform.localScale *= bubbleScale;
        }

        private float GetExplosionRadius()
        {
            if (Global.GameProgress == null)
            {
                return explosionRadius;
            }

            return explosionRadius * Mathf.Max(0.1f, Global.GameProgress.PlayerState.projectileExplosionRadiusModifier);
        }
    }
}
