using System;
using System.Collections.Generic;

namespace GameStart.Persistence
{
    [Serializable]
    public class SaveData
    {
        public int version = SaveSystem.CurrentVersion;
        public List<SkillXpEntry> skillXp = new List<SkillXpEntry>();
        public List<SavedSlot> hotbarSlots = new List<SavedSlot>();
        public List<SavedSlot> mainSlots = new List<SavedSlot>();
        public int gems;
        public int dungeonClearedCount;
        public List<SavedObjective> questObjectives = new List<SavedObjective>();
    }

    [Serializable]
    public class SkillXpEntry
    {
        public string skill;
        public float xp;
    }

    [Serializable]
    public class SavedSlot
    {
        public string itemName;
        public float itemWeight;
        public int count;
    }

    [Serializable]
    public class SavedObjective
    {
        public string description;
        public int targetCount;
        public int currentCount;
    }
}
