using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using TMPEffects.Components;
using UnityEngine;

namespace GlobalSpace
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private GameObject arrow1Fuel;
        [SerializeField] private GameObject arrow2;
        [SerializeField] private GameObject arrow3;
        [SerializeField] private GameObject arrow4Tool;
        [SerializeField] private GameObject character;
        [SerializeField] private float characterJumpHeight = 26f;
        [SerializeField] private float characterBounceHeight = 8f;
        [SerializeField] private float characterJumpDuration = 0.16f;
        [SerializeField] private float characterFallDuration = 0.14f;
        [SerializeField] private float characterBounceDuration = 0.2f;

        private float _timeScaleBeforeTutorial = 1f;
        private bool _upgradeSelectorStateBeforeTutorial;
        private bool _isGameplayPausedForTutorial;
        private TMPWriter _tutorialWriter;
        private bool _tutorialWriterUseScaledTimeBeforePause = true;
        private bool _tutorialWriterTimeOverrideApplied;
        private bool _writerCallbacksBound;
        private Transform _characterTransform;
        private RectTransform _characterRectTransform;
        private Vector3 _characterBaseLocalPosition;
        private Vector2 _characterBaseAnchoredPosition;
        private bool _characterPoseCached;
        private Tween _characterJumpTween;

        private void Awake()
        {
            Global.TutorialController = this;
            ResolveTutorialWriter();
            BindWriterCallbacks();
            CacheCharacterPose();
        }

        public async UniTask StartTutorial()
        {
            if (Global.GameProgress.tutorialPassed) return;

            ResolveTutorialWriter();
            BindWriterCallbacks();
            PauseGameplayForTutorial();
            EnsureCharacterIdlePose();

            root.SetActive(true);
            arrow1Fuel.SetActive(false);
            arrow2.SetActive(false);
            arrow3.SetActive(false);
            arrow4Tool.SetActive(false);
            text.text = "Aye, Captain!";
            await Global.TextController.WaitClickOrSkip(text);

            text.text = "It's our time rich the sea of stars!";
            await Global.TextController.WaitClickOrSkip(text);

            text.text = "Don't forget about fuel and restore it before it's gone!";
            arrow1Fuel.SetActive(true);
            await Global.TextController.WaitClickOrSkip(text);
            arrow1Fuel.SetActive(false);

            text.text = "Here is different sonar signals, use your tools to handle theme";
            arrow2.SetActive(true);
            arrow3.SetActive(true);
            await Global.TextController.WaitClickOrSkip(text);
            arrow2.SetActive(false);
            arrow3.SetActive(false);

            text.text = "here is your tools. Harpoon hooks target and cannon destroy it";
            arrow4Tool.gameObject.SetActive(true);
            await Global.TextController.WaitClickOrSkip(text);
            text.text = "Use left mouse button to fire and right mouse button to switch between tools";
            await Global.TextController.WaitClickOrSkip(text);
            arrow4Tool.gameObject.SetActive(false);
            root.SetActive(false);

            Global.GameProgress.tutorialPassed = true;
            ResumeGameplayAfterTutorial();
        }

        private void OnDisable()
        {
            StopCharacterAnimation(true);
            UnbindWriterCallbacks();
            ResumeGameplayAfterTutorial();
        }

        private void OnDestroy()
        {
            StopCharacterAnimation(true);
            UnbindWriterCallbacks();
            ResumeGameplayAfterTutorial();
        }

        private void PauseGameplayForTutorial()
        {
            if (_isGameplayPausedForTutorial)
            {
                return;
            }

            _isGameplayPausedForTutorial = true;
            _timeScaleBeforeTutorial = Time.timeScale;
            _upgradeSelectorStateBeforeTutorial = Global.IsUpgradeSelectorOpen;

            ApplyTutorialWriterUnscaledTime();
            Time.timeScale = 0f;
            Global.IsUpgradeSelectorOpen = true;
        }

        private void ResumeGameplayAfterTutorial()
        {
            if (!_isGameplayPausedForTutorial)
            {
                return;
            }

            StopCharacterAnimation(true);
            Time.timeScale = _timeScaleBeforeTutorial;
            Global.IsUpgradeSelectorOpen = _upgradeSelectorStateBeforeTutorial;
            RestoreTutorialWriterScaledTime();
            _isGameplayPausedForTutorial = false;
        }

        private void ApplyTutorialWriterUnscaledTime()
        {
            ResolveTutorialWriter();
            BindWriterCallbacks();
            if (_tutorialWriterTimeOverrideApplied || _tutorialWriter == null)
            {
                return;
            }

            _tutorialWriterUseScaledTimeBeforePause = _tutorialWriter.UseScaledTime;
            _tutorialWriter.UseScaledTime = false;
            _tutorialWriterTimeOverrideApplied = true;
        }

        private void RestoreTutorialWriterScaledTime()
        {
            if (!_tutorialWriterTimeOverrideApplied || _tutorialWriter == null)
            {
                return;
            }

            _tutorialWriter.UseScaledTime = _tutorialWriterUseScaledTimeBeforePause;
            _tutorialWriterTimeOverrideApplied = false;
        }

        private void ResolveTutorialWriter()
        {
            _tutorialWriter ??= text != null ? text.GetComponent<TMPWriter>() : null;
        }

        private void BindWriterCallbacks()
        {
            if (_writerCallbacksBound)
            {
                return;
            }

            ResolveTutorialWriter();
            if (_tutorialWriter == null)
            {
                return;
            }

            _tutorialWriter.OnStartWriter.AddListener(HandleWriterStarted);
            _writerCallbacksBound = true;
        }

        private void UnbindWriterCallbacks()
        {
            if (!_writerCallbacksBound || _tutorialWriter == null)
            {
                return;
            }

            _tutorialWriter.OnStartWriter.RemoveListener(HandleWriterStarted);
            _writerCallbacksBound = false;
        }

        private void HandleWriterStarted(TMPWriter writer)
        {
            PlayCharacterJump();
        }

        private void PlayCharacterJump()
        {
            if (!TryPrepareCharacterAnimation())
            {
                return;
            }

            StopCharacterAnimation(true);

            var baseY = GetCharacterBaseY();
            var peakY = baseY + characterJumpHeight;
            var bounceY = baseY + Mathf.Max(2f, characterBounceHeight);
            var bounceUpDuration = Mathf.Max(0.04f, characterBounceDuration * 0.35f);
            var bounceDownDuration = Mathf.Max(0.05f, characterBounceDuration * 0.65f);

            _characterJumpTween = DOTween.Sequence()
                .SetUpdate(true)
                .Append(CreateCharacterMoveTween(peakY, characterJumpDuration, Ease.OutQuad))
                .Append(CreateCharacterMoveTween(baseY, characterFallDuration, Ease.InQuad))
                .Append(CreateCharacterMoveTween(bounceY, bounceUpDuration, Ease.OutQuad))
                .Append(CreateCharacterMoveTween(baseY, bounceDownDuration, Ease.OutBounce))
                .OnComplete(EnsureCharacterIdlePose);
        }

        private void StopCharacterAnimation(bool restoreImmediately)
        {
            _characterJumpTween?.Kill();
            _characterJumpTween = null;

            if (restoreImmediately)
            {
                EnsureCharacterIdlePose();
            }
        }

        private bool TryPrepareCharacterAnimation()
        {
            CacheCharacterPose();
            return _characterTransform != null;
        }

        private void CacheCharacterPose()
        {
            ResolveCharacter();
            if (_characterPoseCached || _characterTransform == null)
            {
                return;
            }

            _characterBaseLocalPosition = _characterTransform.localPosition;
            if (_characterRectTransform != null)
            {
                _characterBaseAnchoredPosition = _characterRectTransform.anchoredPosition;
            }

            _characterPoseCached = true;
        }

        private void ResolveCharacter()
        {
            if (character == null && root != null)
            {
                var foundTransform = root.transform.Find("char");
                if (foundTransform == null)
                {
                    var allChildren = root.GetComponentsInChildren<Transform>(true);
                    for (var i = 0; i < allChildren.Length; i++)
                    {
                        if (allChildren[i].name != "char")
                        {
                            continue;
                        }

                        foundTransform = allChildren[i];
                        break;
                    }
                }

                if (foundTransform != null)
                {
                    character = foundTransform.gameObject;
                }
            }

            if (character == null)
            {
                _characterTransform = null;
                _characterRectTransform = null;
                return;
            }

            _characterTransform = character.transform;
            _characterRectTransform = character.GetComponent<RectTransform>();
        }

        private void EnsureCharacterIdlePose()
        {
            if (!TryPrepareCharacterAnimation())
            {
                return;
            }

            if (_characterRectTransform != null)
            {
                _characterRectTransform.anchoredPosition = _characterBaseAnchoredPosition;
                return;
            }

            _characterTransform.localPosition = _characterBaseLocalPosition;
        }

        private Tween CreateCharacterMoveTween(float targetY, float duration, Ease ease)
        {
            if (_characterRectTransform != null)
            {
                return _characterRectTransform.DOAnchorPosY(targetY, duration).SetEase(ease).SetUpdate(true);
            }

            return _characterTransform.DOLocalMoveY(targetY, duration).SetEase(ease).SetUpdate(true);
        }

        private float GetCharacterBaseY()
        {
            return _characterRectTransform != null
                ? _characterBaseAnchoredPosition.y
                : _characterBaseLocalPosition.y;
        }
    }
}
