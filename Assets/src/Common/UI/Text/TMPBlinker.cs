using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Common.UI.Text
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPBlinker : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float fadeDuration = 0.15f;
        [SerializeField] private float shownHold = 0.6f;
        [SerializeField] private float hiddenHold = 0.4f;

        private Sequence _seq;

        private void Reset() => text = GetComponent<TextMeshProUGUI>();
        private float _targetAlpha;

        private void OnEnable()
        {
            if (!text) text = GetComponent<TextMeshProUGUI>();
            _targetAlpha = text.alpha;
            _seq = DOTween.Sequence()
                .Append(text.DOFade(_targetAlpha, fadeDuration))
                .AppendInterval(shownHold)
                .Append(text.DOFade(0f, fadeDuration))
                .AppendInterval(hiddenHold)
                .SetLoops(-1)
                .SetUpdate(true);
        }

        private void OnDisable()
        {
            _seq?.Kill();
            var c = text.color; c.a = 1f; text.color = c;
        }
    }
}