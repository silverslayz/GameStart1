using System;
using UnityEngine;
using GameStart.Skills;

namespace GameStart.Dungeons
{
    public class PlayerDungeonProgress : MonoBehaviour
    {
        public event Action<int> DungeonEntered;
        public event Action<int> DungeonCleared;

        public int ClearedCount { get; private set; }
        public int CurrentDungeonIndex => ClearedCount;
        public bool HasClearedAll => ClearedCount >= DungeonRegistry.TotalDungeons;

        public DungeonDefinition CurrentDungeon => DungeonRegistry.Get(Mathf.Min(CurrentDungeonIndex, DungeonRegistry.TotalDungeons - 1));

        public void ResetProgress()
        {
            ClearedCount = 0;
        }

        public bool TryEnterCurrentDungeon(PlayerSkills skills)
        {
            if (HasClearedAll || skills == null)
            {
                return false;
            }

            DungeonDefinition dungeon = CurrentDungeon;
            if (!skills.MeetsRequirement(dungeon.Requirement))
            {
                return false;
            }

            DungeonEntered?.Invoke(dungeon.Index);
            return true;
        }

        public void ClearCurrentDungeon()
        {
            if (HasClearedAll)
            {
                return;
            }

            int clearedIndex = CurrentDungeonIndex;
            ClearedCount++;
            DungeonCleared?.Invoke(clearedIndex);
        }
    }
}
