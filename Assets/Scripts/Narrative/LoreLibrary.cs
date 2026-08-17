using System.Collections.Generic;

namespace GameStart.Narrative
{
    // Section 8 Story Synopsis & Lore, and Section 8.1 Tone Reference.
    // Tone: no prophecy, no chosen one, no epic destiny. Ordinary stakes
    // (food, shelter, trust) under extraordinary rules. Any larger mystery
    // stays background noise - rumors and conflicting theories, not answers.
    public static class LoreLibrary
    {
        // --- Core lore (#49) ---
        public static readonly LoreEntry Aetherfall = new LoreEntry(
            "Aetherfall",
            "It was supposed to be a game. A full-dive VR MMO in early access, same as thousands of others that " +
            "came and went. You logged in on an ordinary night for no reason worth remembering.\n\n" +
            "The servers never logged you out. No error message, no support line, no way back. Just the world, " +
            "now fully real to your senses - hunger that actually gnaws, wounds that actually hurt, cold that " +
            "actually kills. Nobody knows if this is a bug, a hack, or something the developers did on purpose. " +
            "Nobody's coming to fix it.");

        public static readonly LoreEntry TheSystem = new LoreEntry(
            "The System",
            "Health bars, stamina, skill levels, loot tables - it's all still running underneath everything, " +
            "exactly as designed. That's the strange comfort in it: the system doesn't lie, doesn't glitch, " +
            "doesn't play favorites. It just keeps score.\n\n" +
            "The one rule everyone agrees on: complete all 100 of the game's original dungeons, clear the Apex " +
            "Boss gating each one, and the system promises your logout will finally go through. Nobody knows if " +
            "that promise is true. It's the only lead anyone has, so people chase it anyway.");

        public static readonly LoreEntry Haven = new LoreEntry(
            "Haven",
            "The old in-game starting town, and the one place with working shelter, a marketplace, and other " +
            "people. It's not safe - it's just less unsafe than everywhere else.\n\n" +
            "Nothing here restocks itself the way it used to when this was just a game. Food runs out. Structures " +
            "decay. The people who've stuck around have had to actually build a life, not just grind stats. " +
            "That's the whole reason Haven still stands.");

        public static readonly IReadOnlyList<LoreEntry> CoreLore = new[] { Aetherfall, TheSystem, Haven };

        // --- In-world text / rumors / corrupted patch notes (#50) ---
        public static readonly IReadOnlyList<LoreEntry> Rumors = new[]
        {
            new LoreEntry("Patch Notes (corrupted)", "...balance changes to Apex Boss aggro radius. Fixed an issue where  [DATA EXPUNGED]  ...v0.9.4..."),
            new LoreEntry("Scrawled on a wall", "Saw someone clear their 40th dungeon last week. Still here. Still hungry. Don't believe the '100 and you're free' thing without proof."),
            new LoreEntry("Overheard at the market", "My cousin swears the devs are still watching, still patching things behind the scenes. I say if they were, they'd have fixed the hunger tick by now."),
            new LoreEntry("Trader's ledger, torn page", "Gems don't rot. Food does. Learned that the hard way. Trade fast, eat faster."),
            new LoreEntry("Carved into a dungeon gate", "WE ARE NOT PLAYERS ANYMORE"),
            new LoreEntry("Whispered theory", "Some say the hundredth dungeon doesn't exist yet - that the system is still generating it based on how many of us are left. Nobody's tested it. Nobody wants to be the one who's wrong."),
        };

        // --- Tutorial NPC dialogue (#51) ---
        public static readonly IReadOnlyList<string> TutorialGiverDialogue = new[]
        {
            "New arrival. Figured you'd be. The system doesn't tell you anything, so I will: stay alive, and try not to think too hard about how.",
            "There's a board by the gate with jobs on it - things Haven actually needs. Start with the gems. Monsters outside town drop them, and they're worth something to people who still remember what money was for.",
            "Ten gems. That's all I'm asking. You'll figure out the rest - fighting, gathering, cooking so you don't starve - faster by doing than by me talking at you.",
            "One more thing. Don't trust anyone who tells you they know how this ends. Nobody does. We're all just trying to clear the next gate.",
        };

        // --- Apex Boss lore per biome (#52) ---
        private static readonly Dictionary<string, LoreEntry> bossLoreByBiome = new Dictionary<string, LoreEntry>
        {
            { "Sunken Ruins", new LoreEntry("Sunken Ruins", "Half-submerged architecture that never matched any real-world style - assets recycled from a scrapped expansion, if the rumors are true. What guards it now moves like it remembers being flooded.") },
            { "Ashen Wastes", new LoreEntry("Ashen Wastes", "A biome that shouldn't still be rendering - grey, low-poly, like the system stopped finishing it. Its Apex Boss is the only thing in the zone with full detail.") },
            { "Verdant Overgrowth", new LoreEntry("Verdant Overgrowth", "Vegetation that grows faster than it should, choking out the paths between visits. Whatever's guarding the gate here has been alone with it long enough to look like part of it.") },
            { "Frostbound Hollow", new LoreEntry("Frostbound Hollow", "Cold that the system was never supposed to simulate this accurately. Survivors who've cleared it describe the boss less as a monster and more as 'something that got here first.'") },
            { "Scorched Foundry", new LoreEntry("Scorched Foundry", "Old crafting-zone assets, industrial and half-melted. The boss here was probably a tutorial enemy once, scaled up past anything the original design ever intended.") },
            { "Drowned Archive", new LoreEntry("Drowned Archive", "Shelves of unreadable text, water damage that never actually recedes. Some say the corrupted patch notes people find in dungeons all originate here.") },
            { "Howling Steppe", new LoreEntry("Howling Steppe", "Open ground, constant wind, and a boss that's rumored to have killed more parties by exhaustion than by damage.") },
            { "Glasswrought Spire", new LoreEntry("Glasswrought Spire", "A tower built from something between glass and screen static. Reflections in it don't always match who's looking.") },
            { "Rootbound Depths", new LoreEntry("Rootbound Depths", "Roots thick enough to be load-bearing, growing in patterns too deliberate to be natural. Nobody's found where they actually lead.") },
            { "Fractured Coastline", new LoreEntry("Fractured Coastline", "A shoreline that loops back on itself if you walk it too long. The Apex Boss here is the one most survivors refuse to describe afterward.") },
        };

        public static LoreEntry GetBossLore(string biomeName)
        {
            if (bossLoreByBiome.TryGetValue(biomeName, out LoreEntry entry))
            {
                return entry;
            }

            return new LoreEntry(biomeName, "No survivors have documented this boss in detail.");
        }
    }
}
