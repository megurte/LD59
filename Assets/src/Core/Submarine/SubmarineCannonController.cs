using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineCannonController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireCooldown = 0.35f;
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private float muzzleDistance = 0.55f;
        [SerializeField] private float projectileRotationOffset = -90f;

        private float _nextShotTime;

        private void Awake()
        {
            targetCamera ??= Camera.main;
            firePoint ??= transform;
        }

        private void Update()
        {
            if (!CanShoot())
            {
                return;
            }

            if (!Input.GetMouseButton(0) || Time.time < _nextShotTime)
            {
                return;
            }

            Fire();
        }

        private void Fire()
        {
            var origin = firePoint != null ? firePoint.position : transform.position;
            var direction = GetAimDirection(origin);
            var spawnPosition = origin + (Vector3)(direction * muzzleDistance);
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + projectileRotationOffset;
            var rotation = Quaternion.Euler(0f, 0f, angle);

            var projectileInstance = Instantiate(projectilePrefab, spawnPosition, rotation);
            if (projectileInstance.TryGetComponent(out CannonProjectile projectile))
            {
                projectile.Launch(direction, projectileSpeed);
            }
            else if (projectileInstance.TryGetComponent(out Rigidbody2D body))
            {
                body.linearVelocity = direction * projectileSpeed;
            }

            _nextShotTime = Time.time + fireCooldown;
        }

        private Vector2 GetAimDirection(Vector3 origin)
        {
            if (targetCamera == null)
            {
                return transform.up;
            }

            var mousePosition = Input.mousePosition;
            var depth = Mathf.Abs(origin.z - targetCamera.transform.position.z);
            var worldPoint = targetCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, depth));

            worldPoint.z = origin.z;

            var direction = worldPoint - origin;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return transform.up;
            }

            return direction.normalized;
        }

        private bool CanShoot()
        {
            return targetCamera != null
                   && projectilePrefab != null
                   && (Global.ToolController == null || Global.ToolController.IsToolActive(ToolType.Cannon));
        }
    }
}
