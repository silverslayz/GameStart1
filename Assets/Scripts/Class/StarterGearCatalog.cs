using System.Collections.Generic;

namespace GameStart.Class
{
    public static class StarterGearCatalog
    {
        private static readonly Dictionary<PlayerClassType, GearItem[]> Kits = new Dictionary<PlayerClassType, GearItem[]>
        {
            { PlayerClassType.Warrior, new[] { new GearItem("Worn Shortsword", 3f), new GearItem("Wooden Buckler", 4f), new GearItem("Padded Tunic", 5f) } },
            { PlayerClassType.Ranger, new[] { new GearItem("Hunting Bow", 2.5f), new GearItem("Bundle of Arrows", 1.5f), new GearItem("Leather Vest", 4f) } },
            { PlayerClassType.Mage, new[] { new GearItem("Apprentice Wand", 1f), new GearItem("Spell Primer", 1.5f), new GearItem("Cloth Robes", 2.5f) } },
        };

        public static IReadOnlyList<GearItem> GetStarterKit(PlayerClassType classType) => Kits[classType];
    }
}
