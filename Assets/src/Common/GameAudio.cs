using GlobalSpace;
using UnityEngine;

namespace Common
{
    public static class GameAudio
    {
        public const string AmbientClipName = "ambient";
        public const string ExplosionClipName = "explosion";
        public const string SonarClipName = "soner";
        public const string PickUp = "pick";

        private const string BubbleSpawnFrequencyKey = "bubble_spawn";
        private static readonly string[] BubbleSpawnSounds = { "water", "water2", "water3" };

        public static void PlayPickUp(float pitch = 3f)
        {
            Global.AudioController?.PlaySoundWithPitch(PickUp, pitch, 0.3f);
        }

        public static void PlayBubbleSpawn(float volume = 0.18f, float minPitch = 0.94f, float maxPitch = 1.06f)
        {
            Global.AudioController?.PlayRandomSoundFromListAtLimitedFrequency(BubbleSpawnFrequencyKey, 
                0.18f, volume, minPitch, maxPitch, true, BubbleSpawnSounds);
        }

        public static void PlayShoot()
        {
            Global.AudioController.PlaySoundWithEnvelope(ExplosionClipName, 0.42f, 3, 0.01f, 0.18f);
        }

        public static void PlayExplosion(float volume = 0.42f, float minPitch = 0.96f, float maxPitch = 1.04f)
        {
            if (Global.AudioController == null)
            {
                return;
            }

            var pitch = Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));
            Global.AudioController.PlaySoundWithEnvelope(ExplosionClipName, volume, pitch, 0.01f, 0.18f);
        }
    }
}
