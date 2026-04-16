using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Common.Animation
{
    public static class AnimationUtils
    {
        public static Tween TextChangeAnimation(this TextMeshProUGUI tmp, float factor = 1.6f, float inDur = 0.1f, float outDur = 0.15f)
        {
            return tmp.transform
                .DOScale(factor, inDur)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    tmp.transform
                        .DOScale(1f, outDur)
                        .SetEase(Ease.InOutSine);
                });
        }

        public static Tween DissolveAnimation(SpriteRenderer sp, float dur = 0.5f)
        {
            var fadeAmount = -0.1f;
            return DOTween.To(() => fadeAmount, x =>
                {
                    fadeAmount = x;
                    sp.material.SetFloat("_FadeAmount", fadeAmount);
                }, 1f, dur)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => sp.gameObject.SetActive(false));
        }

        public static async UniTask HitAnimation(SpriteRenderer sp, float duration = 0.1f, float peakVal = 0.047f)
        {
            await DOTween.To(
                    () => sp.material.GetFloat("_HitEffectBlend"),
                    val => sp.material.SetFloat("_HitEffectBlend", val),
                    peakVal,
                    duration)
                .SetEase(Ease.OutSine)
                .AsyncWaitForCompletion()
                .AsUniTask();

            await DOTween.To(
                    () => sp.material.GetFloat("_HitEffectBlend"),
                    val => sp.material.SetFloat("_HitEffectBlend", val),
                    0f,
                    duration)
                .SetEase(Ease.InSine)
                .AsyncWaitForCompletion()
                .AsUniTask();
        }
        
        /*public static async UniTask ChangeTextValueAnimationUp(int val, bool dec, TextMeshProUGUI textPrefab)
        {
            textPrefab.text = dec ? TextConstants.Negative(val.ToString()) : TextConstants.Utility(val.ToString());
            textPrefab.transform.localPosition = Vector3.zero;

            textPrefab.transform.DOLocalMoveY(250f, 2).SetEase(Ease.Linear);
            await UniTask.WaitForSeconds(0.5f);
            textPrefab.DOFade(0, 1).SetEase(Ease.OutSine);
            textPrefab.transform.DOScale(Vector3.one * 1.4f, 1).SetEase(Ease.OutCirc);
            Object.Destroy(textPrefab.gameObject, 1.2f);
        }*/
        
        public static async UniTask AwaitOrSkip(this Tween tween, int mouseButton = 0, CancellationToken ct = default)
        {
            if (tween == null || !tween.IsActive()) return;

            var tcs = new UniTaskCompletionSource();
            tween.OnComplete(() => tcs.TrySetResult());
            tween.OnKill(() => tcs.TrySetResult());

            var clicked = false;
            var waitClick = UniTask.WaitUntil(() =>
            {
                var down = Input.GetMouseButtonDown(mouseButton);
                if (down) clicked = true;
                return down;
            }, cancellationToken: ct);

            await UniTask.WhenAny(tcs.Task, waitClick);

            if (clicked && tween.IsActive())
                tween.Complete(true);
        }
    }
}