using System.Collections;
using Common;
using GlobalSpace;
using UnityEngine;

namespace Core.Submarine
{
    public class SubmarineSonarSoundLoop : MonoBehaviour
    {
        [SerializeField] private string sonarClipName = GameAudio.SonarClipName;
        [SerializeField] private float initialDelay = 2.5f;
        [SerializeField] private Vector2 intervalRange = new(7.5f, 11.5f);
        [SerializeField] private float fadeInDuration = 0.04f;
        [SerializeField] private float fadeOutDuration = 0.45f;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.18f;
        [SerializeField] private Vector2 pitchRange = new(0.97f, 1.03f);

        private Coroutine _loopCoroutine;

        private void OnEnable()
        {
            _loopCoroutine = StartCoroutine(LoopSonar());
        }

        private void OnDisable()
        {
            if (_loopCoroutine != null)
            {
                StopCoroutine(_loopCoroutine);
                _loopCoroutine = null;
            }
        }

        private IEnumerator LoopSonar()
        {
            if (initialDelay > 0f)
            {
                yield return new WaitForSeconds(initialDelay);
            }

            while (true)
            {
                if (CanPlaySonar())
                {
                    Global.AudioController?.PlaySoundWithEnvelope(
                        sonarClipName,
                        volume,
                        Random.Range(
                            Mathf.Min(pitchRange.x, pitchRange.y),
                            Mathf.Max(pitchRange.x, pitchRange.y)),
                        fadeInDuration,
                        fadeOutDuration);

                    yield return new WaitForSeconds(Random.Range(
                        Mathf.Min(intervalRange.x, intervalRange.y),
                        Mathf.Max(intervalRange.x, intervalRange.y)));
                }
                else
                {
                    yield return null;
                }
            }
        }

        private static bool CanPlaySonar()
        {
            return Global.AudioController != null
                   && Global.SubmarineMovement != null
                   && Global.SubmarineMovement.isActiveAndEnabled
                   && Global.SubmarineMovement.gameObject.activeInHierarchy;
        }
    }
}
