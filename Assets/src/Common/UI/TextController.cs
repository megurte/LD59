using Cysharp.Threading.Tasks;
using GlobalSpace;
using TMPEffects.Components;
using TMPro;
using UnityEngine;

namespace Common.UI
{
    public class TextController : MonoBehaviour
    {
        private TMPWriter _currentWriter;
        private bool _moveToNextText;

        private void Awake()
        {
            Global.TextController = this;
        }

        private void Update()
        {
            if (_currentWriter == null) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                if (_currentWriter.IsWriting)
                    _currentWriter.SkipWriter();
                else
                    _moveToNextText = true;

            }
        }

        public void Reset()
        {
            _moveToNextText = false;
        }
        
        public async UniTask WaitClickOrSkip(TextMeshProUGUI currentText)
        {
            _currentWriter = currentText.GetComponent<TMPWriter>();
            if (_currentWriter == null)
            {
                Debug.LogError("[TextController] TMP has no writer component.");
                return;
            }
            await UniTask.WaitUntil(()=> _moveToNextText);
            _moveToNextText = false;
        }
    }
}