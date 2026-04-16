using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace GlobalSpace
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private bool skipInto;
        [SerializeField] private GameObject intro;
        [SerializeField] private TextMeshProUGUI textLbl;

        public async void Start()
        {
            /*if (Global.gameProgress.victory)
            {
                intro.SetActive(true);
                views.ForEach(v => v.SetInteractable(false));
                await WriteText("Thank you for playing!", 5, false);
                return;
            }*/
            Global.GameProgress = new GameProgress();
            if (skipInto || Global.GameProgress.skipIntro)
            {
                intro.SetActive(false);
                return;
            }

            //views.ForEach(v => v.SetInteractable(false));

            await WriteText("Made by megurt", 4, "pencil1");
            await WriteText("In 48 hours for LD59", 5, "pencil2");
            //Global.AudioController.PlaySoundWithFade("caset_in", 1.5f);
            //await UniTask.WaitForSeconds(0.7f);

            //Global.audioController.SetLoop("vhs_sound");
            //Global.audioController.SetLoopVolume(0.5f);
            //Global.audioController.SetLooping(true);
            //Global.audioController.RestartLoop();

            Global.GameProgress.skipIntro = true;
            intro.GetComponent<CanvasGroup>().DOFade(0, 0.4f).SetEase(Ease.OutSine).OnComplete(()=>intro.SetActive(false));
        }

        private async UniTask WriteText(string tx, int typeCount, string key, bool withFade = true)
        {
            textLbl.color = new Color(1, 1, 1, 1);
            textLbl.text = tx;
            Global.AudioController.PlaySoundWithPitch(key, 1.6f, 0.05f);
            
            for (var i = 0; i < typeCount; i++)
            {
                await UniTask.WaitForSeconds(0.135f);
            }

            await UniTask.WaitForSeconds(1.5f);

            if (withFade)
                await textLbl.DOFade(0, 0.7f).SetEase(Ease.OutSine).AsyncWaitForCompletion().AsUniTask();
        }
    }
}