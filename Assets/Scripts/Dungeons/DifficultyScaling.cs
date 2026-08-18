namespace GameStart.Dungeons
{
    public static class DifficultyScaling
    {
        private const float BaseHealth = 100f;
        private const float HealthPerDungeonIndex = 20f;
        private const float HealthPerPlayerLevel = 5f;

        private const float BaseDamage = 8f;
        private const float DamagePerDungeonIndex = 1.5f;
        private const float DamagePerPlayerLevel = 0.5f;

        public static float GetBossMaxHealth(int dungeonIndex, int playerCombatLevel)
        {
            return BaseHealth + HealthPerDungeonIndex * dungeonIndex + HealthPerPlayerLevel * playerCombatLevel;
        }

        public static float GetBossAttackDamage(int dungeonIndex, int playerCombatLevel)
        {
            return BaseDamage + DamagePerDungeonIndex * dungeonIndex + DamagePerPlayerLevel * playerCombatLevel;
        }

        // Regular monsters scale by MULTIPLIER, where bosses scale to an absolute value.
        //
        // A boss is a set-piece: its numbers can be dictated outright. Regular monsters are
        // authored per type and per archetype (#108, #179), so replacing their values would
        // flatten a tanky wolf and a fast wolf into the same creature at every tier.
        // Multiplying preserves whatever differentiation the prefab defines.
        private const float MonsterHealthPerDungeonIndex = 0.06f;
        private const float MonsterHealthPerPlayerLevel = 0.02f;

        private const float MonsterDamagePerDungeonIndex = 0.04f;
        private const float MonsterDamagePerPlayerLevel = 0.015f;

        /// <summary>Multiplier applied to a monster prefab's authored max health.</summary>
        public static float GetMonsterHealthMultiplier(int dungeonIndex, int playerCombatLevel)
        {
            return 1f
                 + MonsterHealthPerDungeonIndex * dungeonIndex
                 + MonsterHealthPerPlayerLevel * playerCombatLevel;
        }

        /// <summary>Multiplier applied to a monster prefab's authored attack damage.</summary>
        public static float GetMonsterDamageMultiplier(int dungeonIndex, int playerCombatLevel)
        {
            return 1f
                 + MonsterDamagePerDungeonIndex * dungeonIndex
                 + MonsterDamagePerPlayerLevel * playerCombatLevel;
        }
    }
}
