using System;
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

        private List<ToolType> _order = new(){ToolType.Harpoon, ToolType.Cannon};
        private ToolType _currentActiveTool;
        
        private void Awake()
        {
            GlobalSpace.Global.ToolController = this;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                SwitchToolNext();
            }
        }

        public void SwitchToolNext()
        {
            var next = (_order.IndexOf(_currentActiveTool) + 1) % _order.Count;
            _currentActiveTool = _order[next];

            switch (_currentActiveTool)
            {
                case ToolType.Harpoon:
                    Debug.Log("Harpoon");
                    harpoonTransform.gameObject.SetActive(true);
                    cannonTransform.gameObject.SetActive(false);
                    
                    harpoonUI.gameObject.SetActive(true);
                    cannonUI.gameObject.SetActive(false);
                    break;
                case ToolType.Cannon:
                    Debug.Log("Cannon");
                    cannonTransform.gameObject.SetActive(true);
                    harpoonTransform.gameObject.SetActive(false);
                    
                    cannonUI.gameObject.SetActive(true);
                    harpoonUI.gameObject.SetActive(false);
                    break;
            }
        }
    }
}