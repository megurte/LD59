using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.UI
{
    public class HoverScaleOnPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float hoverScaleMultiplier = 1.08f;
        [SerializeField] private float enterDuration = 0.2f;
        [SerializeField] private float exitDuration = 0.16f;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private Color idleColor;
        [SerializeField] private Color selectColor;
        [SerializeField] private Image back;
        
        private Vector3 _initialScale;
        private Tween _scaleTween;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            back.color = idleColor;
            transform.localScale = _initialScale;
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            transform.localScale = _initialScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            back.color = selectColor;
            AnimateScale(_initialScale * hoverScaleMultiplier, enterDuration, Ease.OutBack);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            back.color = idleColor;
            AnimateScale(_initialScale, exitDuration, Ease.OutSine);
        }

        private void AnimateScale(Vector3 targetScale, float duration, Ease ease)
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(targetScale, duration)
                .SetUpdate(useUnscaledTime)
                .SetEase(ease);
        }
    }
}
