using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    [RequireComponent(typeof(Image))]
    public class ImageBlink : MonoBehaviour
    {
        [SerializeField] private float minAlpha = 0.3f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private Ease ease = Ease.InOutSine;

        private Image _image;
        private Tween _blinkTween;

        private void OnEnable()
        {
            _image = GetComponent<Image>();
            StartBlink();
        }

        private void OnDisable()
        {
            _blinkTween?.Kill();
        }

        private void StartBlink()
        {
            var color = _image.color;
            color.a = maxAlpha;
            _image.color = color;

            _blinkTween = DOTween.To(
                    () => _image.color.a,
                    a =>
                    {
                        var c = _image.color;
                        c.a = a;
                        _image.color = c;
                    },
                    minAlpha,
                    duration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}