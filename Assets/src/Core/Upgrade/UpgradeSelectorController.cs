using System.Collections.Generic;
using GlobalSpace;
using UnityEngine;

namespace Core.Upgrade
{
    public class UpgradeSelectorController : MonoBehaviour
    {
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private List<UpgradeSelectorCardView> cardViews = new(3);
        [SerializeField] private CanvasGroup windowCanvasGroup;

        private float _timeScaleBeforeShow = 1f;
        private bool _isVisible;

        private void Awake()
        {
            windowRoot ??= gameObject;
            if (windowCanvasGroup == null && windowRoot != null)
            {
                windowCanvasGroup = windowRoot.GetComponent<CanvasGroup>();
            }

            Global.UpgradeSelectorController = this;
            HideWindowInstant();
        }

        private void OnDestroy()
        {
            if (Global.UpgradeSelectorController == this)
            {
                Global.UpgradeSelectorController = null;
            }

            if (Global.IsUpgradeSelectorOpen)
            {
                ResumeGameplay();
            }
        }

        public void ShowUpgradeSelector(List<IUpgrade> data)
        {
            if (data == null || data.Count != 3)
            {
                Debug.LogError("UpgradeSelectorController expects exactly 3 upgrades.");
                return;
            }

            if (cardViews.Count != 3)
            {
                Debug.LogError("UpgradeSelectorController expects exactly 3 card views.");
                return;
            }

            for (var i = 0; i < cardViews.Count; i++)
            {
                var cardView = cardViews[i];
                if (cardView == null)
                {
                    Debug.LogError($"UpgradeSelectorController card view at index {i} is not assigned.");
                    return;
                }

                cardView.Bind(data[i], SelectUpgrade);
            }

            if (windowRoot != null)
            {
                windowRoot.SetActive(true);
            }

            SetWindowState(true);
            PauseGameplay();
            _isVisible = true;
        }

        public void HideUpgradeSelector()
        {
            if (!_isVisible)
            {
                return;
            }

            HideWindowInstant();
            ResumeGameplay();
        }

        private void SelectUpgrade(IUpgrade upgrade)
        {
            if (upgrade == null)
            {
                return;
            }

            upgrade.Execute();
            HideUpgradeSelector();
        }

        private void HideWindowInstant()
        {
            SetWindowState(false);

            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }

            _isVisible = false;
        }

        private void SetWindowState(bool isVisible)
        {
            if (windowCanvasGroup == null)
            {
                return;
            }

            windowCanvasGroup.alpha = isVisible ? 1f : 0f;
            windowCanvasGroup.interactable = isVisible;
            windowCanvasGroup.blocksRaycasts = isVisible;
        }

        private void PauseGameplay()
        {
            if (Global.IsUpgradeSelectorOpen)
            {
                return;
            }

            _timeScaleBeforeShow = Time.timeScale;
            Time.timeScale = 0f;
            Global.IsUpgradeSelectorOpen = true;
        }

        private void ResumeGameplay()
        {
            Time.timeScale = _timeScaleBeforeShow;
            Global.IsUpgradeSelectorOpen = false;
        }
    }
}
