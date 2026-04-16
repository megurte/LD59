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
                .SetEase(Ease.OutBack);
        }

        protected void AnimateScaleBack()
        {
            _currentTween?.Kill();
            transform.DOKill();

            _currentTween = transform.DOScale(defaultScale, exitDuration)
                .SetEase(Ease.OutBack);
        }
    }
}