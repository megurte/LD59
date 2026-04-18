using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI
{
    public class ButtonComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float defaultScale = 1f;
        [SerializeField] private float hoverDuration = 0.4f;
        [SerializeField] private float exitDuration = 0.3f;
        [SerializeField] private bool useUnscaledTime;
        
        private Tween _currentTween;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            AnimateScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AnimateScaleBack();
        }
        
        protected void AnimateScale()
        {
            _currentTween?.Kill();
            transform.DOKill();

            transform.DOScale(hoverScale, hoverDuration)
                .SetUpdate(useUnscaledTime)
                .SetEase(Ease.OutBack);
        }

        protected void AnimateScaleBack()
        {
            _currentTween?.Kill();
            transform.DOKill();

            _currentTween = transform.DOScale(defaultScale, exitDuration)
                .SetUpdate(useUnscaledTime)
                .SetEase(Ease.OutBack);
        }
    }
}
