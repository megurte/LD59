using Cysharp.Threading.Tasks;
using DG.Tweening;
using GlobalSpace;
using UnityEngine;

namespace Common.UI
{
    public class Fader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float defaultFadeDuration;

        private void Awake()
        {
            Global.GlobalFader = this;
        }

        public async UniTask FadeIn(float duration = default)
        {
            group.alpha = 0;
            await group
                .DOFade(1, duration == 0 ? defaultFadeDuration : duration)
                .AsyncWaitForCompletion()
                .AsUniTask();
        }

        public async UniTask FadeOut(float duration = default)
        {
            group.alpha = 1;
            await group
                .DOFade(0, duration == 0 ? defaultFadeDuration : duration)
                .AsyncWaitForCompletion()
                .AsUniTask();
        }
    }
}