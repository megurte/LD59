using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineWingsAnimator : MonoBehaviour
    {
        [SerializeField] private Transform leftWing;
        [SerializeField] private Transform rightWing;
        [SerializeField] private float angle = 10f;
        [SerializeField] private float speed = 2.4f;
        [SerializeField] private float leftPhase;
        [SerializeField] private float rightPhase = 3.1415927f;

        private Quaternion _leftBaseRotation;
        private Quaternion _rightBaseRotation;

        private void Awake()
        {
            if (leftWing != null)
            {
                _leftBaseRotation = leftWing.localRotation;
            }

            if (rightWing != null)
            {
                _rightBaseRotation = rightWing.localRotation;
            }
        }

        private void OnDisable()
        {
            if (leftWing != null)
            {
                leftWing.localRotation = _leftBaseRotation;
            }

            if (rightWing != null)
            {
                rightWing.localRotation = _rightBaseRotation;
            }
        }

        private void Update()
        {
            var time = Time.time * speed;

            if (leftWing != null)
            {
                var rotation = Mathf.Sin(time + leftPhase) * angle;
                leftWing.localRotation = _leftBaseRotation * Quaternion.Euler(0f, 0f, rotation);
            }

            if (rightWing != null)
            {
                var rotation = Mathf.Sin(time + rightPhase) * angle;
                rightWing.localRotation = _rightBaseRotation * Quaternion.Euler(0f, 0f, rotation);
            }
        }
    }
}
