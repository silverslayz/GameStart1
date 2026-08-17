using UnityEngine;

namespace GameStart.Audio
{
    public class SfxPlayer : MonoBehaviour
    {
        private static SfxPlayer instance;

        private AudioSource source;

        private void Awake()
        {
            instance = this;
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D, UI-friendly
        }

        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (instance == null || clip == null)
            {
                return;
            }

            instance.source.PlayOneShot(clip, volume);
        }
    }
}
