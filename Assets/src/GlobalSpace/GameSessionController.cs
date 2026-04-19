using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlobalSpace
{
public class GameSessionController : MonoBehaviour
    {
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Canvas hudCanvas;
        
        private enum SessionState
        {
            Playing,
            Won,
            Lost
        }
        
        private SessionState _sessionState;

        private void Start()
        {
            _restartButton.onClick.AddListener(RestartGame);
        }

        private void Update()
        {
            if (_sessionState != SessionState.Playing)
            {
                return;
            }

            if (Global.SubmarineMovement.HasReachedRouteEnd)
                TriggerWin();

            if (Global.SubmarineMovement.CurrentFuel <= 0f)
                TriggerLose();
        }

        private void TriggerWin()
        {
            // TODo
            _sessionState = SessionState.Won;
            StopGameplaySystems();
        }

        private void TriggerLose()
        {
            hudCanvas.gameObject.SetActive(false);
            _canvasGroup.gameObject.SetActive(true);
            _canvasGroup.DOFade(1f, 0.6f).SetEase(Ease.OutSine);
            _sessionState = SessionState.Lost;

            Global.GameProgress.PlayerState = new PlayerState();
            Global.UpgradeSelectorController?.HideUpgradeSelector();
            Global.IsUpgradeSelectorOpen = true;

            Global.SubmarineMovement.gameObject.SetActive(false);
            Global.SubmarineCameraController.StopFollow();
            StopGameplaySystems();
            Global.SubmarineMovement.SetTakeableCollectionEnabled(false);
        }

        private void StopGameplaySystems()
        {
            Global.SubmarineMovement.StopMovement();
            Global.FishSpawnController.enabled = false;
            Global.HiddenSignalSpawnController.enabled = false;
        }

        private void RestartGame()
        {
            if (_restartButton != null)
            {
                _restartButton.interactable = false;
            }

            Time.timeScale = 1f;
            Global.IsUpgradeSelectorOpen = false;

            if (Global.GameProgress != null)
            {
                Global.GameProgress.skipIntro = true;
                Global.GameProgress.PlayerState = new PlayerState();
            }

            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}