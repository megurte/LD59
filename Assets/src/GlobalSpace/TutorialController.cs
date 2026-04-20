using System;
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

        private float _timeScaleBeforeTutorial = 1f;
        private bool _upgradeSelectorStateBeforeTutorial;
        private bool _isGameplayPausedForTutorial;
        private TMPWriter _tutorialWriter;
        private bool _tutorialWriterUseScaledTimeBeforePause = true;
        private bool _tutorialWriterTimeOverrideApplied;


        private void Awake()
        {
            Global.TutorialController = this;
        }

        public async UniTask StartTutorial()
        {
            if (Global.GameProgress.tutorialPassed) return;

            PauseGameplayForTutorial();

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
            ResumeGameplayAfterTutorial();
        }

        private void OnDestroy()
        {
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

            Time.timeScale = _timeScaleBeforeTutorial;
            Global.IsUpgradeSelectorOpen = _upgradeSelectorStateBeforeTutorial;
            RestoreTutorialWriterScaledTime();
            _isGameplayPausedForTutorial = false;
        }

        private void ApplyTutorialWriterUnscaledTime()
        {
            if (_tutorialWriterTimeOverrideApplied)
            {
                return;
            }

            _tutorialWriter ??= text.GetComponent<TMPWriter>();
            if (_tutorialWriter == null)
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
     }
}
