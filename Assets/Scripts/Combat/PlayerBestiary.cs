using System;
using System.Collections.Generic;

namespace GameStart.Combat
{
    // Shangri-La Frontier-inspired: repeated engagement with a monster type
    // gradually reveals a weakness, rather than damage being a flat stat check.
    public class PlayerBestiary : UnityEngine.MonoBehaviour
    {
        private const int WeaknessThreshold = 5;
        private const float WeaknessDamageMultiplier = 1.25f;

        private readonly Dictionary<string, int> killCounts = new Dictionary<string, int>();

        public event Action<string> WeaknessDiscovered;
        public event Action<string, int> KillRecorded;

        public IReadOnlyDictionary<string, int> KillCounts => killCounts;

        public void RecordKill(string monsterId)
        {
            if (string.IsNullOrEmpty(monsterId))
            {
                return;
            }

            killCounts.TryGetValue(monsterId, out int count);
            count++;
            killCounts[monsterId] = count;
            KillRecorded?.Invoke(monsterId, count);

            if (count == WeaknessThreshold)
            {
                WeaknessDiscovered?.Invoke(monsterId);
            }
        }

        public int GetKillCount(string monsterId)
        {
            return killCounts.TryGetValue(monsterId, out int count) ? count : 0;
        }

        public bool IsWeaknessDiscovered(string monsterId)
        {
            return GetKillCount(monsterId) >= WeaknessThreshold;
        }

        public float GetDamageMultiplier(string monsterId)
        {
            return IsWeaknessDiscovered(monsterId) ? WeaknessDamageMultiplier : 1f;
        }
    }
}
