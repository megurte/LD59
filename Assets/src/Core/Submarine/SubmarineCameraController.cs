using DG.Tweening;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineCameraController : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 defaultOffset = new(0f, 0f, -10f);
        [SerializeField] private Vector3 farOffset = new(0f, 1.5f, -10f);
        [SerializeField] private float followSmoothTime = 0.35f;
        [SerializeField] private float sizeSmoothSpeed = 6f;
        [SerializeField] private float farOrthographicSize = 8f;
        [SerializeField] private float stateTransitionDuration = 0.7f;
        [SerializeField] private float farStateDuration = 2.5f;

        private Vector3 _followVelocity;
        private float _farStateBlend;
        private float _defaultOrthographicSize;
        private Tween _stateTween;
        private Tween _timerTween;

        private void Awake()
        {
            _defaultOrthographicSize = sceneCamera.orthographicSize;
        }

        private void LateUpdate()
        {
            var desiredOffset = Vector3.Lerp(defaultOffset, farOffset, _farStateBlend);
            var desiredPosition = target.position + desiredOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _followVelocity, followSmoothTime);

            var targetSize = Mathf.Lerp(_defaultOrthographicSize, farOrthographicSize, _farStateBlend);
            sceneCamera.orthographicSize = Mathf.Lerp(sceneCamera.orthographicSize, targetSize, 1f - Mathf.Exp(-sizeSmoothSpeed * Time.deltaTime));
        }

        private void OnDisable()
        {
            _stateTween?.Kill();
            _timerTween?.Kill();
        }

        public void PlayFarState()
        {
            PlayFarState(farStateDuration);
        }

        public void PlayFarState(float duration)
        {
            AnimateFarState(1f);
            _timerTween?.Kill();
            _timerTween = DOVirtual.DelayedCall(duration, () => AnimateFarState(0f));
        }

        public void ReturnToFollow()
        {
            _timerTween?.Kill();
            AnimateFarState(0f);
        }

        private void AnimateFarState(float value)
        {
            _stateTween?.Kill();
            _stateTween = DOVirtual.Float(_farStateBlend, value, stateTransitionDuration, x => _farStateBlend = x)
                .SetEase(Ease.InOutSine);
        }
    }
}
