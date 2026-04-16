using DG.Tweening;
using UnityEngine;

namespace Common.Animation
{
    public class MovementEffect : MonoBehaviour
    {
        [SerializeField] private Transform shadowTransform;
        [SerializeField] private float floatDistance = 2f;
        [SerializeField] private float floatDuration = 2f;
        [SerializeField] private bool inverseMovement;
        
        private Tween _movementTween;
        private Tween _scaleTween;

        private void Start()
        {
            StartFloating();
        }

        public void StartFloating()
        {
            if (_movementTween != null && _movementTween.IsPlaying()) return;

            var startPosition = transform.position;
            var distance = inverseMovement ? -floatDistance : floatDistance;

            _movementTween = transform.DOMoveY(startPosition.y + distance, floatDuration)
                .SetEase(Ease.InOutSine) 
                .SetLoops(-1, LoopType.Yoyo);
            
            if (shadowTransform == null)
            {
                return;
            }

            if (_scaleTween != null && _scaleTween.IsPlaying()) return;

            var originalScale = shadowTransform.localScale;
            var targetScale = originalScale * floatDistance * 1.3f;

            _scaleTween = shadowTransform.DOScale(targetScale, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        public void StopFloating()
        {
            if (_movementTween != null)
            {
                _movementTween.Kill();
                _movementTween = null;
                _scaleTween?.Kill();
                _scaleTween = null;
            }
        }
    }
}