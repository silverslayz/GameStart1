using GameStart.Skills;

namespace GameStart.Dungeons
{
    public readonly struct DungeonDefinition
    {
        public readonly int Index;
        public readonly string Name;
        public readonly string Biome;
        public readonly SkillRequirement Requirement;

        public DungeonDefinition(int index, string name, string biome, SkillRequirement requirement)
        {
            Index = index;
            Name = name;
            Biome = biome;
            Requirement = requirement;
        }
    }
}
