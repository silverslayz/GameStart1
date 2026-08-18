namespace GameStart.Combat
{
    /// <summary>
    /// How many monsters a dungeon should hold, by index.
    ///
    /// Kept as data next to DifficultyScaling so encounter density is tuned in one place
    /// rather than by hand-placing objects in a hundred scenes.
    /// </summary>
    public static class SpawnBudget
    {
        private const int BaseCount = 3;
        private const float CountPerDungeonIndex = 0.08f;
        private const int MaxCount = 12;

        /// <summary>Total monsters a dungeon of this index should sustain.</summary>
        public static int GetMonsterCount(int dungeonIndex)
        {
            int count = BaseCount + (int)(dungeonIndex * CountPerDungeonIndex);
            return count < BaseCount ? BaseCount : (count > MaxCount ? MaxCount : count);
        }

        /// <summary>
        /// Split across spawners so one spawner does not hold a whole dungeon's population.
        /// Always at least one, otherwise a spawner exists but never spawns.
        /// </summary>
        public static int GetPerSpawnerCount(int dungeonIndex, int spawnerCount)
        {
            if (spawnerCount <= 0)
            {
                return 0;
            }

            int total = GetMonsterCount(dungeonIndex);
            int per = total / spawnerCount;
            return per < 1 ? 1 : per;
        }
    }
}
