namespace GameStart.Skills
{
    public static class SkillLevelCurve
    {
        public const int MaxLevel = 99;
        private const float XpPerLevelStep = 50f;

        public static float GetXpRequiredForLevel(int level)
        {
            if (level <= 1)
            {
                return 0f;
            }

            int stepsBelow = level - 1;
            return XpPerLevelStep * stepsBelow * (stepsBelow + 1) / 2f;
        }

        public static int GetLevelForXp(float totalXp)
        {
            int level = 1;
            while (level < MaxLevel && totalXp >= GetXpRequiredForLevel(level + 1))
            {
                level++;
            }

            return level;
        }

        public static float GetProgressToNextLevel(float totalXp)
        {
            int level = GetLevelForXp(totalXp);
            if (level >= MaxLevel)
            {
                return 1f;
            }

            float currentLevelXp = GetXpRequiredForLevel(level);
            float nextLevelXp = GetXpRequiredForLevel(level + 1);
            return (totalXp - currentLevelXp) / (nextLevelXp - currentLevelXp);
        }
    }
}
