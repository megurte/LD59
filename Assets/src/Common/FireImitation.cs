using UnityEngine;

namespace Common
{
    /*public class FireImitation : MonoBehaviour
    {
        [SerializeField] private float flickerSpeed = 5f;
        [SerializeField] private float flickerIntensity = 0.2f;
        [SerializeField] private float minAlpha = 0.3f;
        [SerializeField] private float maxAlpha = 1.0f;
    
        private Light2D _lightSource;
        private float _randomOffset;
        private float _baseSize;
        private Color _baseColor;

        private void Start()
        {
            _lightSource = GetComponent<Light2D>();
            _baseSize = _lightSource.size;
            _baseColor = _lightSource.color;
            _randomOffset = Random.Range(0f, 100f); 
        }
        
        private void FixedUpdate()
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + _randomOffset, 0);
            float flicker = noise * flickerIntensity;

            _lightSource.size = _baseSize + flicker;

            float alpha = Mathf.Lerp(minAlpha, maxAlpha, noise);
            Color newColor = _baseColor;
            newColor.a = alpha;
            _lightSource.color = newColor;
        }
    }  */ 
}