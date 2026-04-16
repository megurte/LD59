using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Systems
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider volumeSoundSlider;
        [SerializeField] private Slider volumeMusicSlider;

        private const string Sound = "Sound";
        private const string Music = "Music";
        
        private void Start()
        {
            if (volumeSoundSlider == null || volumeMusicSlider == null)
            {
                Debug.LogError("Unassigned sliders");
                return;
            }
            
            var savedSound = PlayerPrefs.GetFloat(Sound, 0.75f);
            var savedMusic = PlayerPrefs.GetFloat(Music, 0.75f);
            volumeSoundSlider.value = savedSound;
            volumeMusicSlider.value = savedMusic;
            SetSoundVolume(savedSound);
            SetMusicVolume(savedMusic);
            volumeSoundSlider.onValueChanged.AddListener(SetSoundVolume);
            volumeMusicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        private void SetSoundVolume(float volume)
        {
            var value = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat(Sound, value);
            PlayerPrefs.SetFloat(Sound, volume);
            PlayerPrefs.Save();
        }
        
        private void SetMusicVolume(float volume)
        {
            var value = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat(Music, value);
            PlayerPrefs.SetFloat(Music, volume);
            PlayerPrefs.Save();
        }
    }
}