using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using Core.Submarine;
using DG.Tweening;
using GlobalSpace;

namespace Common
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioController : MonoBehaviour
    {
        public string LoopName
        {
            get
            {
                var clip = GetComponent<AudioSource>().clip;
                if (clip != null)
                {
                    return clip.name;
                }
                else return null;
            }
        }

        public float LoopVolume
        {
            get
            {
                if (source.clip != null)
                {
                    return source.volume;
                }
                else
                {
                    return 0f;
                }
            }
        }

        public float LoopTimeNormalized
        {
            get
            {
                if (source != null && source.clip != null)
                {
                    return 1f - ((source.clip.length - source.time) / source.clip.length);
                }
                else
                {
                    return 0f;
                }
            }
        }

        private List<AudioClip> SFX = new List<AudioClip>();
        private List<AudioClip> Loops = new List<AudioClip>();
        private AudioSource source;

        
        public string InitialSong;
        public float loopOffset;

        public bool Fading { get; set; }

        private string currentSong = "";

        private Dictionary<string, float> limitedFrequencySounds = new Dictionary<string, float>();
        private Dictionary<string, int> lastPlayedSounds = new Dictionary<string, int>();

        private List<AudioMixer> loadedMixers = new List<AudioMixer>();
        private AudioMixerGroup currentSFXMixer;

        private Coroutine skippingCoroutine;

        private const string NULL_INITIAL_SONG = "null";
        
        [SerializeField] private bool autoPlayGameplayAmbient = true;
        [SerializeField] private string gameplayAmbientLoop = GameAudio.AmbientClipName;
        [SerializeField] [Range(0f, 1f)] private float gameplayAmbientVolume = 0.32f;
        [SerializeField] private float gameplayAmbientFadeSpeed = 0.18f;
        [SerializeField] private float gameplayAmbientPitch = 1f;
        [SerializeField] private bool autoAddGameplaySonar = true;

        private void Awake()
        {
            Global.AudioController = this;
            source = GetComponent<AudioSource>();

            foreach (object o in Resources.LoadAll("CMS/SFX"))
            {
                if (o is not AudioClip clip) continue;
                SFX.Add(clip);
            }

            foreach (object o in Resources.LoadAll("CMS/SFX/Loops"))
            {
                if (o is not AudioClip clip) continue;
                Loops.Add(clip);
            }

            if (InitialSong != NULL_INITIAL_SONG)
            {
                CrossFadeLoop(InitialSong, 1f, 5f, 1f, loopOffset);
            }
        }

        private void Start()
        {
            if (!HasGameplaySubmarine())
            {
                return;
            }

            if (autoPlayGameplayAmbient
                && string.IsNullOrWhiteSpace(InitialSong)
                && !string.IsNullOrWhiteSpace(gameplayAmbientLoop)
                && GetLoop(gameplayAmbientLoop) != null)
            {
                CrossFadeLoopIfNotPlaying(
                    gameplayAmbientLoop,
                    gameplayAmbientVolume,
                    gameplayAmbientFadeSpeed,
                    gameplayAmbientPitch);
            }

            if (autoAddGameplaySonar
                && GetSFX(GameAudio.SonarClipName) != null
                && GetComponent<SubmarineSonarSoundLoop>() == null)
            {
                gameObject.AddComponent<SubmarineSonarSoundLoop>();
            }
        }

        public AudioSource PlayClipAt(AudioClip clip, Vector3 pos)
        {
            GameObject tempGO = new GameObject("TempAudio");
            tempGO.transform.position = pos;

            AudioSource aSource = tempGO.AddComponent<AudioSource>();
            aSource.clip = clip;
            aSource.outputAudioMixerGroup = currentSFXMixer;

            aSource.Play();
            Destroy(tempGO, clip.length);
            return aSource;
        }

        public AudioSource PlaySoundWithEnvelope(
            string soundName,
            float volume = 1f,
            float pitch = 1f,
            float fadeInDuration = 0f,
            float fadeOutDuration = 0f)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                return null;
            }

            var clip = GetSFX(soundName);
            if (clip == null)
            {
                return null;
            }

            var tempGO = new GameObject("TempAudioEnvelope");
            var audioSource = tempGO.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = currentSFXMixer;
            audioSource.pitch = Mathf.Approximately(pitch, 0f) ? 1f : pitch;

            var targetVolume = Mathf.Max(0f, volume);
            audioSource.volume = fadeInDuration > 0f ? 0f : targetVolume;
            audioSource.Play();

            if (fadeInDuration > 0f)
            {
                audioSource.DOFade(targetVolume, fadeInDuration).SetEase(Ease.OutSine);
            }

            var playbackDuration = clip.length / Mathf.Max(0.01f, Mathf.Abs(audioSource.pitch));
            if (fadeOutDuration > 0f)
            {
                var fadeOutDelay = Mathf.Max(fadeInDuration, playbackDuration - fadeOutDuration);
                DOVirtual.DelayedCall(fadeOutDelay, () =>
                    {
                        if (audioSource != null)
                        {
                            audioSource.DOFade(0f, fadeOutDuration).SetEase(Ease.InSine);
                        }
                    })
                    .SetLink(tempGO);
            }

            Destroy(tempGO, playbackDuration + 0.15f);
            return audioSource;
        }

        public AudioSource PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                return null;
            }

            AudioClip sound = SFX.Find(x => x.name == soundName);

            if (sound != null)
            {
                return PlayClipAt(sound, Vector3.zero);
            }

            return null;
        }

        public AudioSource PlaySoundAtLimitedFrequency(string soundName, float frequencyMin, float pitch = 1f,
            float volume = 1f, string entrySuffix = "")
        {
            float time = Time.unscaledTime;
            string soundKey = soundName + entrySuffix;

            if (limitedFrequencySounds.ContainsKey(soundKey))
            {
                if (time - frequencyMin > limitedFrequencySounds[soundKey])
                {
                    limitedFrequencySounds[soundKey] = time;
                    return PlaySoundWithPitch(soundName, pitch, volume);
                }
            }
            else
            {
                limitedFrequencySounds.Add(soundKey, time);
                return PlaySoundWithPitch(soundName, pitch, volume);
            }

            return null;
        }

        public AudioSource PlayRandomSound(string soundPrefix, bool noRepeating = false, bool randomPitch = false, float volume = 1)
        {
            List<AudioClip> variations = SFX.FindAll(x => x != null && x.name.StartsWith(soundPrefix + "_"));

            string soundId = "";

            if (variations.Count > 0)
            {
                int index = Random.Range(0, variations.Count) + 1;
                if (noRepeating)
                {
                    if (!lastPlayedSounds.ContainsKey(soundPrefix))
                    {
                        lastPlayedSounds.Add(soundPrefix, index);
                    }
                    else
                    {
                        int breakOutCounter = 0;
                        const int BREAK_OUT_THRESHOLD = 100;
                        while (lastPlayedSounds[soundPrefix] == index && breakOutCounter < BREAK_OUT_THRESHOLD)
                        {
                            index = Random.Range(0, variations.Count) + 1;
                            breakOutCounter++;
                        }

                        if (breakOutCounter >= BREAK_OUT_THRESHOLD - 1)
                        {
                            Debug.Log("Broke out of infinite loop! AudioController.PlayRandomSound");
                        }

                        lastPlayedSounds[soundPrefix] = index;
                    }
                }

                soundId = soundPrefix + "_" + index;
            }
            else
            {
                soundId = soundPrefix;
            }

            AudioSource randomSound = PlaySoundWithPitch(soundId, randomPitch ? Random.Range(0.9f, 1.1f) : 1f, volume);

            if (randomSound == null)
            {
                return PlaySoundWithPitch(soundPrefix, 1f, volume);
            }
            else
            {
                return randomSound;
            }
        }

        public void PlaySoundDelayed(string soundName, float delay, float pitch = 1f, float volume = 1f)
        {
            StartCoroutine(WaitThenPlaySound(soundName, delay, pitch, volume));
        }

        IEnumerator WaitThenPlaySound(string soundName, float delay, float pitch, float volume)
        {
            yield return new WaitForSeconds(delay);
            PlaySoundWithPitch(soundName, pitch, volume);
        }

        public AudioSource PlaySoundWithPitch(string soundName, float pitch, float volume)
        {
            if (!string.IsNullOrEmpty(soundName))
            {
                AudioSource a = PlaySound(soundName);
                if (a != null)
                {
                    a.pitch *= pitch;
                    a.volume *= volume;
                }

                return a;
            }
            else
            {
                return null;
            }
        }

        public AudioSource PlaySoundMuffled(string soundName, float cutoff = 300f)
        {
            var sound = PlaySound(soundName);
            var filter = sound.gameObject.AddComponent<AudioLowPassFilter>();
            filter.cutoffFrequency = cutoff;
            return sound;
        }

        public void SetLooping(bool looping)
        {
            source.loop = looping;
        }

        public void PauseLoop()
        {
            source.Pause();
        }

        public void ResumeLoop(float fadeInSpeed = float.MaxValue)
        {
            source.UnPause();

            if (!source.isPlaying)
            {
                source.Play();
            }

            source.volume = 0f;
            FadeInLoop(fadeInSpeed);
        }

        public void RestartLoop()
        {
            source.Stop();
            source.Play();
            Fading = false;
        }

        public void StopLoop()
        {
            source.Stop();
            source.loop = false;
            currentSong = "";
            Fading = false;
        }

        public AudioClip GetLoop(string loopName)
        {
            var loop = Loops.Find(x => x.name == loopName);
            return loop != null ? loop : GetSFX(loopName);
        }

        public void SetLoop(string loopName)
        {
            currentSong = loopName;
            if (source != null)
            {
                AudioClip loop = GetLoop(loopName);
                if (loop != null)
                {
                    source.Stop();
                    source.time = 0f;
                    source.clip = loop;
                    source.loop = true;
                    source.Play();
                }
            }
        }

        public void ClearLoopMixing()
        {
            source.outputAudioMixerGroup = null;
        }

        public void SetLoopMixing(string mixerId, string groupId)
        {
            source.outputAudioMixerGroup = GetMixerGroup(mixerId, groupId);
        }

        public AudioClip GetSFX(string sfxName)
        {
            return SFX.Find(x => x.name == sfxName);
        }

        public AudioSource PlayRandomSoundFromList(
            float volume = 1f,
            float minPitch = 1f,
            float maxPitch = 1f,
            bool noRepeating = true,
            string historyKey = "",
            params string[] soundNames)
        {
            var availableSounds = new List<string>();
            foreach (var soundName in soundNames)
            {
                if (!string.IsNullOrEmpty(soundName) && GetSFX(soundName) != null)
                {
                    availableSounds.Add(soundName);
                }
            }

            if (availableSounds.Count == 0)
            {
                return null;
            }

            var key = string.IsNullOrEmpty(historyKey) ? string.Join("|", availableSounds) : historyKey;
            var index = Random.Range(0, availableSounds.Count);

            if (noRepeating && availableSounds.Count > 1)
            {
                if (lastPlayedSounds.TryGetValue(key, out var lastIndex) && lastIndex == index)
                {
                    index = (index + Random.Range(1, availableSounds.Count)) % availableSounds.Count;
                }

                lastPlayedSounds[key] = index;
            }

            var pitch = Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));
            return PlaySoundWithPitch(availableSounds[index], pitch, volume);
        }

        public AudioSource PlayRandomSoundFromListAtLimitedFrequency(
            string soundKey,
            float frequencyMin,
            float volume = 1f,
            float minPitch = 1f,
            float maxPitch = 1f,
            bool noRepeating = true,
            params string[] soundNames)
        {
            if (string.IsNullOrEmpty(soundKey))
            {
                return PlayRandomSoundFromList(volume, minPitch, maxPitch, noRepeating, soundKey, soundNames);
            }

            var time = Time.unscaledTime;
            if (limitedFrequencySounds.TryGetValue(soundKey, out var lastPlayTime)
                && time - lastPlayTime < frequencyMin)
            {
                return null;
            }

            limitedFrequencySounds[soundKey] = time;
            return PlayRandomSoundFromList(volume, minPitch, maxPitch, noRepeating, soundKey, soundNames);
        }

        public void ClearSFXMixing()
        {
            currentSFXMixer = null;
        }

        public void SetSFXMixing(string mixerId, string groupId)
        {
            currentSFXMixer = GetMixerGroup(mixerId, groupId);
        }

        public void FadeLoopIfNotPlaying(string loopName, float volume)
        {
            if (loopName != currentSong)
            {
                SetLoop(loopName);
                SetLoopVolume(volume);
                FadeInLoop(1f);
            }
        }

        public void CrossFadeLoopIfNotPlaying(string loopName, float volume, float speed = 0.5f, float pitch = 1f)
        {
            if (currentSong != loopName)
            {
                CrossFadeLoop(loopName, volume, speed, pitch);
            }
        }

        public void CrossFadeLoop(string loopName, float volume = 1f, float speed = 5f, float pitch = 1f,
            float offset = 0f)
        {
            if (GetLoop(loopName) == null)
            {
                return;
            }

            currentSong = loopName;
            StopAllCoroutines();
            StartCoroutine(Cross(loopName, volume, false, speed, pitch, offset));
        }

        public void FadeOutLoop(float fadeSpeed = float.MaxValue)
        {
            StopAllCoroutines();
            StartCoroutine(DoFadeToVolume(fadeSpeed, 0f));
        }

        public void FadeInLoop(float fadeSpeed = float.MaxValue, float toVolume = 1f)
        {
            StopAllCoroutines();
            StartCoroutine(DoFadeToVolume(fadeSpeed, toVolume));
        }

        IEnumerator DoFadeToVolume(float fadeSpeed, float newNormalizedVolume)
        {
            Fading = true;
            if (source.volume > GetAdjustedVolume(newNormalizedVolume))
            {
                //Fading Down
                while (source.volume >= GetAdjustedVolume(newNormalizedVolume))
                {
                    source.volume -= GetAdjustedVolume(Time.deltaTime * fadeSpeed);
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                //Fading Up
                while (source.volume <= GetAdjustedVolume(newNormalizedVolume))
                {
                    source.volume += GetAdjustedVolume(Time.deltaTime * fadeSpeed);
                    yield return new WaitForEndOfFrame();
                }
            }

            source.volume = GetAdjustedVolume(newNormalizedVolume);
            Fading = false;
        }

        public void FadeOutThenPlayAfterDelay(string bgm, float delay)
        {
            FadeOutLoop();
            StartCoroutine(WaitThenPlay(bgm, delay));
        }

        IEnumerator WaitThenPlay(string bgm, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            CrossFadeLoop(bgm, 1f);
        }

        IEnumerator Cross(string newLoop, float volume, bool fadeOut, float speed = 5f, float pitch = 1f,
            float offset = 0f)
        {
            Fading = true;
            //If a loop is playing, fade it out
            if (source.clip && source.isPlaying)
            {
                while (source.volume > 0f)
                {
                    source.volume -= (speed * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }

                source.volume = 0f;
            }

            if (!fadeOut)
            {
                //Fade in the new loop to be played
                SetLoop(newLoop);
                SetLoopVolume(0);
                source.pitch = pitch;
                source.time = offset;
                while (source.volume < GetAdjustedVolume(volume))
                {
                    source.volume += (speed * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }

                source.volume = GetAdjustedVolume(volume);
            }

            Fading = false;
        }

        public void SetLoopCutoff(float cutoff)
        {
            var filter = GetComponent<AudioLowPassFilter>();
            if (filter == null)
            {
                filter = gameObject.AddComponent<AudioLowPassFilter>();
            }

            filter.cutoffFrequency = cutoff;
        }

        public void SetLoopVolume(float volume, bool stopFades = false)
        {
            if (stopFades)
            {
                StopAllCoroutines();
            }

            source.volume = volume;
        }

        public void SetLoopVolume(float volume, float speed)
        {
            StopAllCoroutines();
            StartCoroutine(DoFadeToVolume(speed, volume));
        }

        public void SetLoopTimeNormalized(float time)
        {
            if (source.clip != null)
            {
                float newTime = source.clip.length * time;
                if (newTime < source.clip.length)
                {
                    source.time = newTime;
                }
            }
        }

        public void StartLoopSkipping(float skipLength)
        {
            StopLoopSkipping();
            skippingCoroutine = StartCoroutine(DoLoopSkipping(source.time, skipLength));
        }

        public void StopLoopSkipping()
        {
            if (skippingCoroutine != null)
            {
                StopCoroutine(skippingCoroutine);
            }
        }

        public void StopCrossFade()
        {
            StopAllCoroutines();
        }

        public AudioMixerGroup GetMixerGroup(string mixerId, string groupId)
        {
            var loadedMixer = loadedMixers.Find(x => x.name == mixerId);
            if (loadedMixer == null)
            {
                loadedMixer = Resources.Load<AudioMixer>("CMS/SFX/Mixing/" + mixerId);
                loadedMixers.Add(loadedMixer);
            }

            if (loadedMixer != null)
            {
                var groups = loadedMixer.FindMatchingGroups(groupId);
                if (groups.Length > 0)
                {
                    return groups[0];
                }
            }

            return null;
        }

        public IEnumerator FadeMixerParameter(string mixer, string channel, string parameter, float fadeTo,
            float fadeLength)
        {
            var mainBGM = GetMixerGroup(mixer, channel);
            float startVolume;
            mainBGM.audioMixer.GetFloat(parameter, out startVolume);
            float timer = 0f;

            while (timer < fadeLength)
            {
                timer += Time.deltaTime;
                mainBGM.audioMixer.SetFloat(parameter, Mathf.Lerp(startVolume, fadeTo, timer / fadeLength));
                yield return new WaitForEndOfFrame();
            }

            mainBGM.audioMixer.SetFloat(parameter, fadeTo);
        }

        public IEnumerator FadeOutExternalSource(AudioSource externalSource, float fadeSpeed)
        {
            while (externalSource != null && externalSource.volume > 0f)
            {
                externalSource.volume -= Time.deltaTime * fadeSpeed;
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator DoLoopSkipping(float startTime, float skipLength)
        {
            while (true)
            {
                yield return new WaitForSeconds(skipLength);
                source.time = startTime;
            }
        }

        private float GetAdjustedVolume(float volume)
        {
            return volume;
        }

        public void PlaySoundWithFade(string sound, float fadeDur = 0.2f)
        {
            if (string.IsNullOrEmpty(sound)) return;
            var clip = GetSFX(sound);
            if (clip == null) return;
            var src = PlayClipAt(clip, Vector3.zero);
            if (src == null) return;

            StartCoroutine(PlaySfxWithFadeTail(src, clip.length, fadeDur));
        }

        private IEnumerator PlaySfxWithFadeTail(AudioSource src, float clipLength, float fadeDuration)
        {
            if (src == null) yield break;

            var delay = Mathf.Max(0f, clipLength - fadeDuration);
            yield return new WaitForSeconds(delay);
            var startVol = src.volume;
            var fadeSpeed = (fadeDuration > 0f) ? (startVol / fadeDuration) : startVol;
            yield return FadeOutExternalSource(src, fadeSpeed);
        }

        public AudioSource PlayRandomSoundWithPitch(string soundPrefix, float pitch, float volume = 1)
        {
            List<AudioClip> variations = SFX.FindAll(x => x != null && x.name.StartsWith(soundPrefix + "_"));

            string soundId = "";

            if (variations.Count > 0)
            {
                int index = Random.Range(0, variations.Count) + 1;
                if (true)
                {
                    if (!lastPlayedSounds.ContainsKey(soundPrefix))
                    {
                        lastPlayedSounds.Add(soundPrefix, index);
                    }
                    else
                    {
                        int breakOutCounter = 0;
                        const int BREAK_OUT_THRESHOLD = 100;
                        while (lastPlayedSounds[soundPrefix] == index && breakOutCounter < BREAK_OUT_THRESHOLD)
                        {
                            index = Random.Range(0, variations.Count) + 1;
                            breakOutCounter++;
                        }

                        if (breakOutCounter >= BREAK_OUT_THRESHOLD - 1)
                        {
                            Debug.Log("Broke out of infinite loop! AudioController.PlayRandomSound");
                        }

                        lastPlayedSounds[soundPrefix] = index;
                    }
                }

                soundId = soundPrefix + "_" + index;
            }
            else
            {
                soundId = soundPrefix;
            }

            AudioSource randomSound = PlaySoundWithPitch(soundId, pitch, volume);

            if (randomSound == null)
            {
                return PlaySoundWithPitch(soundPrefix, pitch, volume);
            }
            else
            {
                return randomSound;
            }
        }

        private bool HasGameplaySubmarine()
        {
            return Global.SubmarineMovement != null || FindFirstObjectByType<SubmarineMovementController>() != null;
        }
    }
}
