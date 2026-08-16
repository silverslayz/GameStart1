namespace GameStart.Skills
{
    [System.Serializable]
    public struct SkillRequirement
    {
        public SkillType Skill;
        public int RequiredLevel;

        public SkillRequirement(SkillType skill, int requiredLevel)
        {
            Skill = skill;
            RequiredLevel = requiredLevel;
        }
    }
}
