using UnityEngine;

namespace Core.Fish
{
    public class FishBoneAnimator : MonoBehaviour
    {
        [SerializeField] private FishMovementController movement;
        [SerializeField] private Transform rootVisual;
        [SerializeField] private Transform[] spineBones;
        [SerializeField] private Transform[] leftFinBones;
        [SerializeField] private Transform[] rightFinBones;
        [SerializeField] private float idleFrequency = 1.25f;
        [SerializeField] private float swimFrequency = 3.1f;
        [SerializeField] private float idleAmplitude = 4f;
        [SerializeField] private float swimAmplitude = 18f;
        [SerializeField] private float secondaryWaveFrequency = 1.75f;
        [SerializeField] private float secondaryWaveFactor = 0.2f;
        [SerializeField] private float phaseOffset = 1.25f;
        [SerializeField] private float turnAngle = 10f;
        [SerializeField] private float rootSwayOffset = 0.04f;
        [SerializeField] private float rootSwayAngle = 3.5f;
        [SerializeField] private float rootTurnAngle = 5f;
        [SerializeField] private float finFrequency = 4.2f;
        [SerializeField] private float finAngle = 18f;
        [SerializeField] private float finTurnAngle = 12f;
        [SerializeField] private float finPhaseOffset = 0.55f;
        [SerializeField] private float animationResponse = 8f;
        [SerializeField] private AnimationCurve spineAmplitudeCurve = new(
            new Keyframe(0f, 0.08f),
            new Keyframe(0.28f, 0.18f),
            new Keyframe(0.62f, 0.55f),
            new Keyframe(1f, 1f));
        [SerializeField] private AnimationCurve spineTurnCurve = new(
            new Keyframe(0f, 0.05f),
            new Keyframe(0.35f, 0.2f),
            new Keyframe(0.7f, 0.55f),
            new Keyframe(1f, 1f));

        private Quaternion[] _spineBaseRotations = new Quaternion[0];
        private Quaternion[] _leftFinBaseRotations = new Quaternion[0];
        private Quaternion[] _rightFinBaseRotations = new Quaternion[0];
        private Vector3 _rootBasePosition;
        private Quaternion _rootBaseRotation;
        private float _currentFrequency;
        private float _currentAmplitude;
        private float _speedBlend;
        private float _turnBlend;

        private void Awake()
        {
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
            var targetSpeed = movement != null ? movement.SpeedNormalized : 1f;
            var targetTurn = movement != null ? movement.TurnAmount : 0f;
            var blend = 1f - Mathf.Exp(-animationResponse * Time.deltaTime);

            _speedBlend = Mathf.Lerp(_speedBlend, targetSpeed, blend);
            _turnBlend = Mathf.Lerp(_turnBlend, targetTurn, blend);
            _currentFrequency = Mathf.Lerp(_currentFrequency, Mathf.Lerp(idleFrequency, swimFrequency, _speedBlend), blend);
            _currentAmplitude = Mathf.Lerp(_currentAmplitude, Mathf.Lerp(idleAmplitude, swimAmplitude, _speedBlend), blend);

            var time = Time.time * _currentFrequency;

            AnimateRoot(time);
            AnimateSpine(time);
            AnimateFins(leftFinBones, _leftFinBaseRotations, 1f, time);
            AnimateFins(rightFinBones, _rightFinBaseRotations, -1f, time + 0.35f);
        }

        private void OnDisable()
        {
            RestorePose();
        }

        private void AnimateRoot(float time)
        {
            if (rootVisual == null)
            {
                return;
            }

            var swimFactor = Mathf.Lerp(0.35f, 1f, _speedBlend);
            var wave = Mathf.Sin(time + 0.25f);
            var roll = Mathf.Cos(time + 0.2f) * rootSwayAngle * swimFactor;

            rootVisual.localPosition = _rootBasePosition + Vector3.right * (wave * rootSwayOffset * swimFactor - _turnBlend * rootSwayOffset);
            rootVisual.localRotation = _rootBaseRotation * Quaternion.Euler(0f, 0f, roll - _turnBlend * rootTurnAngle);
        }

        private void AnimateSpine(float time)
        {
            if (spineBones == null || _spineBaseRotations == null)
            {
                return;
            }

            for (var i = 0; i < spineBones.Length; i++)
            {
                var bone = spineBones[i];
                if (bone == null || i >= _spineBaseRotations.Length)
                {
                    continue;
                }

                var t = spineBones.Length <= 1 ? 1f : i / (float)(spineBones.Length - 1);
                var amplitudeWeight = spineAmplitudeCurve.Evaluate(t);
                var turnWeight = spineTurnCurve.Evaluate(t);
                var wave = Mathf.Sin(time - t * phaseOffset);
                var secondaryWave = Mathf.Sin(time * secondaryWaveFrequency - t * phaseOffset * 1.35f + 0.6f);
                var angle = (wave + secondaryWave * secondaryWaveFactor) * _currentAmplitude * amplitudeWeight;

                angle -= _turnBlend * turnAngle * turnWeight;
                bone.localRotation = _spineBaseRotations[i] * Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void AnimateFins(Transform[] fins, Quaternion[] baseRotations, float sideSign, float time)
        {
            if (fins == null || baseRotations == null)
            {
                return;
            }

            var swimFactor = Mathf.Lerp(0.45f, 1f, _speedBlend);
            var flapFrequency = finFrequency * Mathf.Lerp(0.8f, 1.15f, _speedBlend);

            for (var i = 0; i < fins.Length; i++)
            {
                var fin = fins[i];
                if (fin == null || i >= baseRotations.Length)
                {
                    continue;
                }

                var flap = Mathf.Sin(time * flapFrequency + i * finPhaseOffset) * finAngle * swimFactor;
                var turnOffset = -_turnBlend * finTurnAngle * sideSign;

                fin.localRotation = baseRotations[i] * Quaternion.Euler(0f, 0f, flap * sideSign + turnOffset);
            }
        }

        private void CachePose()
        {
            if (rootVisual != null)
            {
                _rootBasePosition = rootVisual.localPosition;
                _rootBaseRotation = rootVisual.localRotation;
            }

            _spineBaseRotations = CaptureRotations(spineBones);
            _leftFinBaseRotations = CaptureRotations(leftFinBones);
            _rightFinBaseRotations = CaptureRotations(rightFinBones);
        }

        private void RestorePose()
        {
            if (rootVisual != null)
            {
                rootVisual.localPosition = _rootBasePosition;
                rootVisual.localRotation = _rootBaseRotation;
            }

            RestoreRotations(spineBones, _spineBaseRotations);
            RestoreRotations(leftFinBones, _leftFinBaseRotations);
            RestoreRotations(rightFinBones, _rightFinBaseRotations);
        }

        private static Quaternion[] CaptureRotations(Transform[] bones)
        {
            if (bones == null)
            {
                return new Quaternion[0];
            }

            var rotations = new Quaternion[bones.Length];

            for (var i = 0; i < bones.Length; i++)
            {
                rotations[i] = bones[i] != null ? bones[i].localRotation : Quaternion.identity;
            }

            return rotations;
        }

        private static void RestoreRotations(Transform[] bones, Quaternion[] rotations)
        {
            if (bones == null || rotations == null)
            {
                return;
            }

            for (var i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                if (bone == null || i >= rotations.Length)
                {
                    continue;
                }

                bone.localRotation = rotations[i];
            }
        }
    }
}
