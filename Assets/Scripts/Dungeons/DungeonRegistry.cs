using System.Collections.Generic;
using GameStart.Skills;

namespace GameStart.Dungeons
{
    public static class DungeonRegistry
    {
        /// <summary>
        /// Deliberately small until the core loop proves itself. 100 gates over 10 biomes
        /// was a promise the team can't keep: procedurally-named repeats read as filler,
        /// and none of them exist as authored spaces yet.
        ///
        /// 20 is two per biome, so every biome is represented evenly (the biome is picked
        /// by index % Biomes.Length, so multiples of 10 stay balanced). Raise it once a
        /// dungeon is actually fun to play through - not before.
        /// </summary>
        public const int TotalDungeons = 20;

        private static readonly string[] Biomes =
        {
            "Sunken Ruins",
            "Ashen Wastes",
            "Verdant Overgrowth",
            "Frostbound Hollow",
            "Scorched Foundry",
            "Drowned Archive",
            "Howling Steppe",
            "Glasswrought Spire",
            "Rootbound Depths",
            "Fractured Coastline",
        };

        private static readonly IReadOnlyList<DungeonDefinition> Dungeons = BuildDungeons();

        private static IReadOnlyList<DungeonDefinition> BuildDungeons()
        {
            var list = new List<DungeonDefinition>(TotalDungeons);

            for (int index = 0; index < TotalDungeons; index++)
            {
                string biome = Biomes[index % Biomes.Length];
                string name = $"{biome} — Gate {index + 1}";
                int requiredLevel = 1 + (index / 2);
                var requirement = new SkillRequirement(SkillType.Combat, requiredLevel);

                list.Add(new DungeonDefinition(index, name, biome, requirement));
            }

            return list;
        }

        public static DungeonDefinition Get(int index) => Dungeons[index];

        public static bool IsValidIndex(int index) => index >= 0 && index < TotalDungeons;
    }
}
