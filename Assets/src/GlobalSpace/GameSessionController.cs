using Core.Fish;
using Core.SignalObjects;
using Core.Submarine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GlobalSpace
{
    public class GameSessionController : MonoBehaviour
    {
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Canvas hudCanvas;

        // win
        [SerializeField] private GameObject winRoot;
        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private CanvasGroup winCanvasGroup;
        [SerializeField] private TextMeshProUGUI sentence;
        [SerializeField] private TextMeshProUGUI thxText;

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
            {
                TriggerWin();
            }

            if (Global.SubmarineMovement.CurrentFuel <= 0f)
            {
                TriggerLose();
            }
        }

        private void TriggerWin()
        {
            hudCanvas.gameObject.SetActive(false);
            _sessionState = SessionState.Won;
            StopGameplaySystems();
            winRoot.SetActive(true);
            winText.alpha = 0;
            sentence.alpha = 0;
            thxText.alpha = 0;

            var seq = DOTween.Sequence();
            seq.Append(winText.DOFade(1, 1.4f).SetEase(Ease.OutSine));
            seq.Append(winCanvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutSine));
            seq.Append(sentence.DOFade(1, 1.4f).SetEase(Ease.OutSine));
            seq.Append(thxText.DOFade(1, 1.4f).SetEase(Ease.OutSine));
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
            Global.UpgradeSelectorController?.HideUpgradeSelector();
            Global.IsUpgradeSelectorOpen = true;

            Global.SubmarineMovement?.SetTakeableCollectionEnabled(false);
            Global.SubmarineMovement?.StopMovement();

            CleanupSpawnSystems();
            DisableGameplayBehaviours();
            CleanupRuntimeObjects();
            CleanupRuntimeParticles();
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

        private static void CleanupSpawnSystems()
        {
            if (Global.FishSpawnController != null)
            {
                Global.FishSpawnController.ClearSpawnedContent();
                Global.FishSpawnController.enabled = false;
            }

            if (Global.HiddenSignalSpawnController != null)
            {
                Global.HiddenSignalSpawnController.ClearSpawnedContent();
                Global.HiddenSignalSpawnController.enabled = false;
            }
        }

        private static void DisableGameplayBehaviours()
        {
            if (Global.SubmarineCameraController != null)
            {
                Global.SubmarineCameraController.StopFollow();
                Global.SubmarineCameraController.enabled = false;
            }

            DisableBehaviour(Global.ToolController);
            DisableBehaviour(Global.HarpoonController);

            if (Global.SubmarineMovement != null)
            {
                var submarineRoot = Global.SubmarineMovement.transform;
                DisableBehavioursInChildren<ToolController>(submarineRoot);
                DisableBehavioursInChildren<SubmarineCannonController>(submarineRoot);
                DisableBehavioursInChildren<SubmarineHarpoonController>(submarineRoot);
                DisableBehavioursInChildren<OrbitingToolController>(submarineRoot);
                DisableBehavioursInChildren<SubmarineWingsAnimator>(submarineRoot);
            }

            var sonarLoops = Object.FindObjectsByType<SubmarineSonarSoundLoop>(FindObjectsSortMode.None);
            for (var i = 0; i < sonarLoops.Length; i++)
            {
                DisableBehaviour(sonarLoops[i]);
            }
        }

        private static void CleanupRuntimeObjects()
        {
            DestroyObjectsOfType<CannonProjectile>();
            DestroyObjectsOfType<MineProjectile>();
            DestroyObjectsOfType<FishExtract>();
            DestroyObjectsOfType<FishState>();
            DestroyObjectsOfType<FishSphereState>();
            DestroyObjectsOfType<HiddenMineState>();
            DestroyObjectsOfType<HiddenUpgradeState>();
        }

        private void CleanupRuntimeParticles()
        {
            var particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            for (var i = 0; i < particles.Length; i++)
            {
                var particleSystem = particles[i];
                if (particleSystem == null || IsTransformUnderWinRoot(particleSystem.transform))
                {
                    continue;
                }

                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private bool IsTransformUnderWinRoot(Transform target)
        {
            return target != null && winRoot != null && target.IsChildOf(winRoot.transform);
        }

        private static void DisableBehaviour(Behaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = false;
            }
        }

        private static void DisableBehavioursInChildren<T>(Transform root) where T : Behaviour
        {
            if (root == null)
            {
                return;
            }

            var behaviours = root.GetComponentsInChildren<T>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].enabled = false;
            }
        }

        private static void DestroyObjectsOfType<T>() where T : Component
        {
            var objects = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            for (var i = 0; i < objects.Length; i++)
            {
                var target = objects[i];
                if (target == null)
                {
                    continue;
                }

                if (target.gameObject.activeSelf)
                {
                    target.gameObject.SetActive(false);
                }

                Object.Destroy(target.gameObject);
            }
        }
    }
}
