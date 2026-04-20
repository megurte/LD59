using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using TMPEffects.Components;
using UnityEngine;

namespace GlobalSpace
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private bool skipInto;
        [SerializeField] private GameObject intro;
        [SerializeField] private TextMeshProUGUI textLbl;
        [SerializeField] private Canvas HUD;

        private TMPWriter _introWriter;
        private bool _introWriterUseScaledTimeBeforePause = true;
        private bool _introWriterTimeOverrideApplied;

        public async void Start()
        {
            Global.Initialize();
            /*if (skipInto || Global.GameProgress.skipIntro)
            {
                intro.SetActive(false);
                PlayGameplay();
                return;
            }*/
            //else
            {
                PauseGameplay();
                HUD.gameObject.SetActive(false);
                intro.SetActive(true);
            }

            await WriteText("Made by megurt", 4, "pencil1");
            await WriteText("In 48 hours for LD59", 5, "pencil2");

            Global.GameProgress.skipIntro = true;
            await intro.GetComponent<CanvasGroup>()
                .DOFade(0, 0.4f)
                .SetUpdate(true)
                .SetEase(Ease.OutSine)
                .OnComplete(() => intro.SetActive(false))
                .AsyncWaitForCompletion()
                .AsUniTask();

            HUD.gameObject.SetActive(true);
            await Global.TutorialController.StartTutorial();
            PlayGameplay();
        }

        private async UniTask WriteText(string tx, int typeCount, string key, bool withFade = true)
        {
            textLbl.color = new Color(1, 1, 1, 1);
            textLbl.text = tx;
            Global.AudioController.PlaySoundWithPitch(key, 1.6f, 0.05f);

            for (var i = 0; i < typeCount; i++)
            {
                await UniTask.WaitForSeconds(0.135f, ignoreTimeScale: true);
            }

            await UniTask.WaitForSeconds(1.5f, ignoreTimeScale: true);

            if (withFade)
            {
                await textLbl.DOFade(0, 0.7f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutSine)
                    .AsyncWaitForCompletion()
                    .AsUniTask();
            }
        }

        private void PauseGameplay()
        {
            ApplyIntroWriterUnscaledTime();
            Time.timeScale = 0f;
        }

        private void PlayGameplay()
        {
            Time.timeScale = 1f;
            RestoreIntroWriterScaledTime();
        }

        private void OnDisable()
        {
            RestoreIntroWriterScaledTime();
        }

        private void OnDestroy()
        {
            RestoreIntroWriterScaledTime();
        }

        private void ApplyIntroWriterUnscaledTime()
        {
            if (_introWriterTimeOverrideApplied)
            {
                return;
            }

            _introWriter ??= textLbl.GetComponent<TMPWriter>();
            if (_introWriter == null)
            {
                return;
            }

            _introWriterUseScaledTimeBeforePause = _introWriter.UseScaledTime;
            _introWriter.UseScaledTime = false;
            _introWriterTimeOverrideApplied = true;
        }

        private void RestoreIntroWriterScaledTime()
        {
            if (!_introWriterTimeOverrideApplied || _introWriter == null)
            {
                return;
            }

            _introWriter.UseScaledTime = _introWriterUseScaledTimeBeforePause;
            _introWriterTimeOverrideApplied = false;
        }
    }
}
