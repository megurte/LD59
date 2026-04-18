using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Upgrade
{
    public class UpgradeSelectorCardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI descLabel;
        [SerializeField] private Image iconImage;

        private IUpgrade _boundUpgrade;
        private Action<IUpgrade> _onSelected;

        public void Bind(IUpgrade upgrade, Action<IUpgrade> onSelected)
        {
            _boundUpgrade = upgrade;
            _onSelected = onSelected;
            nameLabel.text = upgrade.Name;
            descLabel.text = upgrade.Desc;
            if (upgrade.Icon != null)
                iconImage.sprite = upgrade.Icon;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_boundUpgrade == null)
            {
                return;
            }

            _onSelected?.Invoke(_boundUpgrade);
        }
    }
}
