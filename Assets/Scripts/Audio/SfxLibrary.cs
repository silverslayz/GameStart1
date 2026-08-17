using UnityEngine;

namespace GameStart.Audio
{
    // Simple synthesized placeholder SFX (sine-wave tones with an envelope),
    // generated in-code rather than as imported audio files, until real SFX
    // exist (Section 9 Asset Breakdown: "Combat SFX... UI cues").
    public static class SfxLibrary
    {
        private const int SampleRate = 44100;

        private static AudioClip attackSwing;
        private static AudioClip damageHit;
        private static AudioClip monsterDefeat;
        private static AudioClip levelUp;
        private static AudioClip victory;
        private static AudioClip uiClick;

        public static AudioClip AttackSwing => attackSwing != null ? attackSwing : (attackSwing = MakeSweep("AttackSwing", 900f, 300f, 0.12f, 0.5f));
        public static AudioClip DamageHit => damageHit != null ? damageHit : (damageHit = MakeSweep("DamageHit", 220f, 90f, 0.15f, 0.6f));
        public static AudioClip MonsterDefeat => monsterDefeat != null ? monsterDefeat : (monsterDefeat = MakeSweep("MonsterDefeat", 400f, 80f, 0.35f, 0.5f));
        public static AudioClip LevelUp => levelUp != null ? levelUp : (levelUp = MakeArpeggio("LevelUp", new[] { 523f, 659f, 784f }, 0.09f, 0.4f));
        public static AudioClip Victory => victory != null ? victory : (victory = MakeArpeggio("Victory", new[] { 523f, 659f, 784f, 1047f }, 0.14f, 0.5f));
        public static AudioClip UIClick => uiClick != null ? uiClick : (uiClick = MakeSweep("UIClick", 1400f, 1400f, 0.04f, 0.3f));

        private static AudioClip MakeSweep(string name, float startFreq, float endFreq, float duration, float volume)
        {
            int sampleCount = Mathf.Max(1, (int)(SampleRate * duration));
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += freq / SampleRate;
                float envelope = Mathf.Sin(t * Mathf.PI); // fades in and out
                samples[i] = Mathf.Sin(phase * 2f * Mathf.PI) * envelope * volume;
            }

            return CreateClip(name, samples);
        }

        private static AudioClip MakeArpeggio(string name, float[] noteFrequencies, float noteDuration, float volume)
        {
            int samplesPerNote = Mathf.Max(1, (int)(SampleRate * noteDuration));
            float[] samples = new float[samplesPerNote * noteFrequencies.Length];

            for (int n = 0; n < noteFrequencies.Length; n++)
            {
                float phase = 0f;
                for (int i = 0; i < samplesPerNote; i++)
                {
                    float t = (float)i / samplesPerNote;
                    phase += noteFrequencies[n] / SampleRate;
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    samples[n * samplesPerNote + i] = Mathf.Sin(phase * 2f * Mathf.PI) * envelope * volume;
                }
            }

            return CreateClip(name, samples);
        }

        private static AudioClip CreateClip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
