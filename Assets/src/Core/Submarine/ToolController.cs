using System.Collections.Generic;
using UnityEngine;

namespace Core.Submarine
{
    public enum ToolType
    {
        Harpoon,
        Cannon,
    }

    public class ToolController : MonoBehaviour
    {
        [SerializeField] private Transform harpoonTransform;
        [SerializeField] private Transform cannonTransform;

        [SerializeField] private GameObject harpoonUI;
        [SerializeField] private GameObject cannonUI;

        private readonly List<ToolType> _order = new() { ToolType.Harpoon, ToolType.Cannon };
        private ToolType _currentActiveTool;

        public ToolType CurrentActiveTool => _currentActiveTool;

        private void Awake()
        {
            GlobalSpace.Global.ToolController = this;
        }

        private void Update()
        {
            if (GlobalSpace.Global.IsUpgradeSelectorOpen)
            {
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                SwitchToolNext();
            }
        }

        public void AddNewTool(ToolType toolType)
        {
            if (!_order.Contains(toolType))
            {
                _order.Add(toolType);
            }

            _currentActiveTool = toolType;
        }

        public void SwitchToolNext()
        {
            if (_order.Count == 0)
            {
                return;
            }

            var next = (_order.IndexOf(_currentActiveTool) + 1) % _order.Count;
            _currentActiveTool = _order[next];

            switch (_currentActiveTool)
            {
                case ToolType.Harpoon:
                    harpoonTransform.gameObject.SetActive(true);
                    cannonTransform.gameObject.SetActive(false);

                    harpoonUI.gameObject.SetActive(true);
                    cannonUI.gameObject.SetActive(false);
                    break;
                case ToolType.Cannon:
                    cannonTransform.gameObject.SetActive(true);
                    harpoonTransform.gameObject.SetActive(false);

                    cannonUI.gameObject.SetActive(true);
                    harpoonUI.gameObject.SetActive(false);
                    break;
            }
        }

        public bool IsToolActive(ToolType toolType)
        {
            return _currentActiveTool == toolType;
        }
    }
}
