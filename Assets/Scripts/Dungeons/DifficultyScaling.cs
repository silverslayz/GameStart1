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
    }
}
