using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class OrbitingToolController : MonoBehaviour
    {
        [SerializeField] private ToolType toolType;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform orbitCenter;
        [SerializeField] private float orbitRadius = 0.75f;
        [SerializeField] private float rotationOffset = -90f;

        private Vector3 _lastAimDirection = Vector3.right;

        private void Awake()
        {
            targetCamera ??= Camera.main;
            orbitCenter ??= transform.parent;
        }

        private void Update()
        {
            if (!ShouldUpdate())
            {
                return;
            }

            var center = orbitCenter != null ? orbitCenter.position : transform.position;
            var mousePosition = Input.mousePosition;
            var depth = Mathf.Abs(center.z - targetCamera.transform.position.z);
            var worldPoint = targetCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, depth));
            var aimDirection = worldPoint - center;
            aimDirection.z = 0f;

            if (aimDirection.sqrMagnitude > 0.0001f)
            {
                _lastAimDirection = aimDirection.normalized;
            }

            transform.position = center + _lastAimDirection * orbitRadius;

            var angle = Mathf.Atan2(_lastAimDirection.y, _lastAimDirection.x) * Mathf.Rad2Deg + rotationOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private bool ShouldUpdate()
        {
            if (targetCamera == null)
            {
                return false;
            }
            var toolController = Global.ToolController;

            if (toolController.CurrentActiveTool == ToolType.Harpoon)
            {
                if (Global.HarpoonController.InAir)
                    return false;
            }
            
            return toolController == null || toolController.IsToolActive(toolType);
        }
    }
}
