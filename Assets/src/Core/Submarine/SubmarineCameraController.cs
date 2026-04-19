using System.Collections;
using Constants;
using DG.Tweening;
using GlobalSpace;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Submarine
{
    public class SubmarineCameraController : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 defaultOffset = new(0f, 0f, -10f);
        [SerializeField] private Vector3 boostOffset = new(0f, 2.6f, -10f);
        [SerializeField] private float followSmoothTime = 0.35f;
        [SerializeField] private float sizeSmoothSpeed = 6f;
        [SerializeField] private float farOrthographicSize = 8f;
        [SerializeField] private float boostOrthographicSize = 10f;
        [SerializeField] private float stateTransitionDuration = 0.7f;
        [SerializeField] private float farStateDuration = 2.5f;
       
        // boost fx
        [SerializeField] private float boostEnterDuration = 0.28f;
        [SerializeField] private float boostExitDuration = 0.45f;
        [SerializeField] private float boostShakeStrength = 0.18f;
        [SerializeField] private float boostShakeFrequency = 19f;
        [SerializeField] private float boostMotionBlurIntensity = 0.45f;
        [SerializeField] private float boostDepthBlurRadius = 1.65f;
        [SerializeField] private float boostBloomIntensity = 4.5f;
        [SerializeField] private float boostChromaticAberration = 0.28f;
        [SerializeField] private float boostVignetteIntensity = 0.2f;
        [SerializeField] private float bubbleBurstInterval = 0.06f;
        [SerializeField] private int bubbleBurstsPerPulse = 3;
        [SerializeField] private Vector2 bubbleSpawnRadius = new(2f, 1.25f);
        [SerializeField] private Vector3 bubbleSpawnOffset = new(0f, -0.1f, 0f);
        [SerializeField] private Vector2 bubbleScaleRange = new(0.8f, 1.25f);

        private Vector3 _followVelocity;
        private float _farStateBlend;
        private float _boostBlend;
        private float _shakeBlend;
        private float _defaultOrthographicSize;
        private Tween _stateTween;
        private Tween _timerTween;
        private Tween _boostTween;
        private Tween _boostTimerTween;
        private Tween _shakeTween;
        private Coroutine _bubbleRoutine;
        private MotionBlur _motionBlur;
        private DepthOfField _depthOfField;
        private Bloom _bloom;
        private ChromaticAberration _chromaticAberration;
        private Vignette _vignette;
        private bool _motionBlurWasActive;
        private bool _depthOfFieldWasActive;
        private bool _bloomWasActive;
        private bool _chromaticAberrationWasActive;
        private bool _vignetteWasActive;
        private float _baseMotionBlurIntensity;
        private float _baseDepthBlurRadius;
        private float _baseBloomIntensity;
        private float _baseChromaticAberration;
        private float _baseVignetteIntensity;

        private void Awake()
        {
            _defaultOrthographicSize = sceneCamera.orthographicSize;
            InitializePostProcessing();
        }

        private void OnEnable()
        {
            Global.SubmarineCameraController = this;
        }

        private void LateUpdate()
        {
            var desiredOffset = Vector3.Lerp(defaultOffset, boostOffset, _boostBlend);

            var desiredPosition = target.position + desiredOffset + GetShakeOffset();
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _followVelocity, followSmoothTime);

            var targetSize = Mathf.Lerp(_defaultOrthographicSize, farOrthographicSize, _farStateBlend);
            targetSize = Mathf.Lerp(targetSize, boostOrthographicSize, _boostBlend);
            sceneCamera.orthographicSize = Mathf.Lerp(sceneCamera.orthographicSize, targetSize, 1f - Mathf.Exp(-sizeSmoothSpeed * Time.deltaTime));

            ApplyBoostPostProcessing();
        }

        public void PlayFarState()
        {
            PlayFarState(farStateDuration);
        }

        public void PlayFarState(float duration)
        {
            AnimateFarState(1f);
            _timerTween?.Kill();
            _timerTween = DOVirtual.DelayedCall(duration, () => AnimateFarState(0f));
        }

        public void ReturnToFollow()
        {
            _timerTween?.Kill();
            AnimateFarState(0f);
        }

        public void PlaySpeedBoostState(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            AnimateBoostState(1f, boostEnterDuration);
            AnimateShake(1f, boostEnterDuration * 0.65f);

            _boostTimerTween?.Kill();
            _boostTimerTween = DOVirtual.DelayedCall(duration, EndSpeedBoostFx);

            if (_bubbleRoutine != null)
            {
                StopCoroutine(_bubbleRoutine);
            }

            _bubbleRoutine = StartCoroutine(SpawnBoostBubbles(duration + boostExitDuration * 0.4f));
        }

        private void AnimateFarState(float value)
        {
            _stateTween?.Kill();
            _stateTween = DOVirtual.Float(_farStateBlend, value, stateTransitionDuration, x => _farStateBlend = x)
                .SetEase(Ease.InOutSine);
        }

        private void AnimateBoostState(float value, float duration)
        {
            _boostTween?.Kill();
            _boostTween = DOVirtual.Float(_boostBlend, value, duration, x => _boostBlend = x)
                .SetEase(value > _boostBlend ? Ease.OutCubic : Ease.InOutSine);
        }

        private void AnimateShake(float value, float duration)
        {
            _shakeTween?.Kill();
            _shakeTween = DOVirtual.Float(_shakeBlend, value, duration, x => _shakeBlend = x)
                .SetEase(value > _shakeBlend ? Ease.OutQuad : Ease.InOutSine);
        }

        private void EndSpeedBoostFx()
        {
            AnimateBoostState(0f, boostExitDuration);
            AnimateShake(0f, boostExitDuration);
        }

        private void InitializePostProcessing()
        {
            if (postProcessVolume?.profile == null)
            {
                return;
            }

            var runtimeProfile = postProcessVolume.profile;

            if (runtimeProfile.TryGet(out _motionBlur))
            {
                _motionBlurWasActive = _motionBlur.active;
                _baseMotionBlurIntensity = _motionBlur.intensity.value;
            }

            if (runtimeProfile.TryGet(out _depthOfField))
            {
                _depthOfFieldWasActive = _depthOfField.active;
                _baseDepthBlurRadius = _depthOfField.gaussianMaxRadius.value;
                _depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
            }

            if (runtimeProfile.TryGet(out _bloom))
            {
                _bloomWasActive = _bloom.active;
                _baseBloomIntensity = _bloom.intensity.value;
            }

            if (runtimeProfile.TryGet(out _chromaticAberration))
            {
                _chromaticAberrationWasActive = _chromaticAberration.active;
                _baseChromaticAberration = _chromaticAberration.intensity.value;
            }

            if (runtimeProfile.TryGet(out _vignette))
            {
                _vignetteWasActive = _vignette.active;
                _baseVignetteIntensity = _vignette.intensity.value;
            }
        }

        private void ApplyBoostPostProcessing()
        {
            _motionBlur.active = _motionBlurWasActive || _boostBlend > 0.001f;
            _motionBlur.intensity.value = Mathf.Lerp(_baseMotionBlurIntensity, boostMotionBlurIntensity, _boostBlend);
            
            _depthOfField.active = _depthOfFieldWasActive || _boostBlend > 0.001f;
            _depthOfField.gaussianMaxRadius.value = Mathf.Lerp(_baseDepthBlurRadius, boostDepthBlurRadius, _boostBlend);
            
            _bloom.active = _bloomWasActive || _boostBlend > 0.001f;
            _bloom.intensity.value = Mathf.Lerp(_baseBloomIntensity, boostBloomIntensity, _boostBlend);

            _chromaticAberration.active = _chromaticAberrationWasActive || _boostBlend > 0.001f;
            _chromaticAberration.intensity.value = Mathf.Lerp(_baseChromaticAberration, boostChromaticAberration, _boostBlend);

            _vignette.active = _vignetteWasActive || _boostBlend > 0.001f;
            _vignette.intensity.value = Mathf.Lerp(_baseVignetteIntensity, boostVignetteIntensity, _boostBlend);
        }

        private Vector3 GetShakeOffset()
        {
            if (_shakeBlend <= 0.001f)
            {
                return Vector3.zero;
            }

            var time = Time.time * boostShakeFrequency;
            var x = (Mathf.PerlinNoise(time, 0.17f) - 0.5f) * 2f;
            var y = (Mathf.PerlinNoise(0.41f, time) - 0.5f) * 2f;
            return new Vector3(x, y, 0f) * (boostShakeStrength * _shakeBlend);
        }

        private IEnumerator SpawnBoostBubbles(float duration)
        {
            var bubbleBurst = Global.EffectFactory.LoadVFX(Models.BubbleBurst);
            var endTime = Time.time + duration;
            
            while (Time.time < endTime)
            {
                for (var i = 0; i < bubbleBurstsPerPulse; i++)
                {
                    SpawnBubbleBurst(bubbleBurst);
                }

                yield return new WaitForSeconds(bubbleBurstInterval);
            }

            _bubbleRoutine = null;
        }

        private void SpawnBubbleBurst(ParticleSystem bubbleBurst)
        {
            var randomOffset = new Vector3(
                Random.Range(-bubbleSpawnRadius.x, bubbleSpawnRadius.x),
                Random.Range(-bubbleSpawnRadius.y, bubbleSpawnRadius.y),
                0f);

            var burst = Instantiate(bubbleBurst, target.position + bubbleSpawnOffset + randomOffset, Quaternion.identity);
            burst.transform.localScale *= Random.Range(bubbleScaleRange.x, bubbleScaleRange.y);
        }
        
        private void OnDisable()
        {
            _stateTween?.Kill();
            _timerTween?.Kill();
            _boostTween?.Kill();
            _boostTimerTween?.Kill();
            _shakeTween?.Kill();
            _boostBlend = 0f;
            _shakeBlend = 0f;
            ApplyBoostPostProcessing();

            if (_bubbleRoutine != null)
            {
                StopCoroutine(_bubbleRoutine);
                _bubbleRoutine = null;
            }

            Global.SubmarineCameraController = null;
        }
        
        private void OnDestroy()
        {
            Global.SubmarineCameraController = null;
        }
    }
}
