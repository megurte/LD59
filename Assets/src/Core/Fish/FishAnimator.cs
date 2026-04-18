using UnityEngine;

namespace Core.Fish
{
    // FISH FIIIIIISH FIIIIISH
    public class FishAnimator : MonoBehaviour
    {
        private static readonly int WaveAmplitudeId = Shader.PropertyToID("_WaveAmplitude");
        private static readonly int TailAmplitudeId = Shader.PropertyToID("_TailAmplitude");
        private static readonly int WaveFrequencyId = Shader.PropertyToID("_WaveFrequency");
        private static readonly int WavePhaseId = Shader.PropertyToID("_WavePhase");
        private static readonly int HeadInfluenceId = Shader.PropertyToID("_HeadInfluence");
        private static readonly int TailPowerId = Shader.PropertyToID("_TailPower");

        [SerializeField] private FishMovementController movement;
        [SerializeField] private Transform spriteTransform;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float idleAmplitude = 0.014f;
        [SerializeField] private float moveAmplitude = 0.05f;
        [SerializeField] private float idleTailAmplitude = 0.012f;
        [SerializeField] private float moveTailAmplitude = 0.055f;
        [SerializeField] private float idleFrequency = 1.5f;
        [SerializeField] private float moveFrequency = 3.4f;
        [SerializeField] private float wavePhase = 7.8f;
        [SerializeField] private float headInfluence = 0.03f;
        [SerializeField] private float tailPower = 1.9f;
        [SerializeField] private float bodyOffset = 0.055f;
        [SerializeField] private float bodyAngle = 7.5f;
        [SerializeField] private float turnAngle = 10f;
        [SerializeField] private float scaleFactor = 0.045f;
        [SerializeField] private float animationResponse = 8.5f;

        private MaterialPropertyBlock _propertyBlock;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private Vector3 _baseLocalScale;
        private float _currentAmplitude;
        private float _currentTailAmplitude;
        private float _currentFrequency;
        private float _turnBlend;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            CachePose();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            CachePose();
        }

        private void LateUpdate()
        {
            if (spriteTransform == null || spriteRenderer == null)
            {
                return;
            }

            var speed = movement != null ? movement.SpeedNormalized : 1f;
            var turn = movement != null ? movement.TurnAmount : 0f;
            var blend = 1f - Mathf.Exp(-animationResponse * Time.deltaTime);

            _currentAmplitude = Mathf.Lerp(_currentAmplitude, Mathf.Lerp(idleAmplitude, moveAmplitude, speed), blend);
            _currentTailAmplitude = Mathf.Lerp(_currentTailAmplitude, Mathf.Lerp(idleTailAmplitude, moveTailAmplitude, speed), blend);
            _currentFrequency = Mathf.Lerp(_currentFrequency, Mathf.Lerp(idleFrequency, moveFrequency, speed), blend);
            _turnBlend = Mathf.Lerp(_turnBlend, turn, blend);

            var time = Time.time * _currentFrequency;
            var wave = Mathf.Sin(time);
            var swimFactor = Mathf.Lerp(0.35f, 1f, speed);
            var bodyShift = wave * bodyOffset * swimFactor;
            var bodyRotation = Mathf.Cos(time) * bodyAngle * swimFactor;
            var turnRotation = -_turnBlend * turnAngle;
            var scaleOffset = Mathf.Abs(wave) * scaleFactor * Mathf.Lerp(0.2f, 1f, speed);

            spriteTransform.localPosition = _baseLocalPosition + Vector3.right * (bodyShift - _turnBlend * bodyOffset * 0.75f);
            spriteTransform.localRotation = _baseLocalRotation * Quaternion.Euler(0f, 0f, bodyRotation + turnRotation);
            spriteTransform.localScale = _baseLocalScale + new Vector3(-scaleOffset, 0f, 0f);

            // :poop:
            spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(WaveAmplitudeId, _currentAmplitude);
            _propertyBlock.SetFloat(TailAmplitudeId, _currentTailAmplitude);
            _propertyBlock.SetFloat(WaveFrequencyId, _currentFrequency);
            _propertyBlock.SetFloat(WavePhaseId, wavePhase);
            _propertyBlock.SetFloat(HeadInfluenceId, headInfluence);
            _propertyBlock.SetFloat(TailPowerId, tailPower);
            spriteRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDisable()
        {
            if (spriteTransform != null)
            {
                spriteTransform.localPosition = _baseLocalPosition;
                spriteTransform.localRotation = _baseLocalRotation;
                spriteTransform.localScale = _baseLocalScale;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.SetPropertyBlock(null);
            }
        }

        private void CachePose()
        {
            if (spriteTransform == null)
            {
                return;
            }

            _baseLocalPosition = spriteTransform.localPosition;
            _baseLocalRotation = spriteTransform.localRotation;
            _baseLocalScale = spriteTransform.localScale;
        }
    }
}
