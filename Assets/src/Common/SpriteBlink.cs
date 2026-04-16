using DG.Tweening;
using UnityEngine;

namespace Common
{
    public class SpriteBlink : MonoBehaviour
    {
        [SerializeField] private float minAlpha = 0.3f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float duration = 0.8f;

        private SpriteRenderer _spriteRenderer;
        private Tween _blinkTween;

        private void OnEnable()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            StartBlink();
        }

        private void OnDisable()
        {
            _blinkTween?.Kill();
        }

        private void StartBlink()
        {
            var color = _spriteRenderer.color;
            color.a = maxAlpha;
            _spriteRenderer.color = color;
            
            _blinkTween = DOTween.To(
                    () => _spriteRenderer.color.a,
                    a =>
                    {
                        var c = _spriteRenderer.color;
                        c.a = a;
                        _spriteRenderer.color = c;
                    },
                    minAlpha,
                    duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}