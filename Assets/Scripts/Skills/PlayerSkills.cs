using System;
using System.Collections.Generic;
using UnityEngine;
using GameStart.Class;

namespace GameStart.Skills
{
    public class PlayerSkills : MonoBehaviour
    {
        private readonly Dictionary<SkillType, float> xpBySkill = new Dictionary<SkillType, float>();

        public event Action<SkillType, float, int> SkillXpChanged;
        public event Action<SkillType, int> SkillLeveledUp;
        public event Action AllSkillsReset;

        private PlayerSkillTree skillTree;

        private void Awake()
        {
            skillTree = GetComponent<PlayerSkillTree>();
        }

        public float GetXp(SkillType skill) => xpBySkill.TryGetValue(skill, out float xp) ? xp : 0f;

        public int GetLevel(SkillType skill) => SkillLevelCurve.GetLevelForXp(GetXp(skill));

        public bool MeetsRequirement(SkillRequirement requirement) => GetLevel(requirement.Skill) >= requirement.RequiredLevel;

        public bool MeetsRequirements(IEnumerable<SkillRequirement> requirements)
        {
            foreach (SkillRequirement requirement in requirements)
            {
                if (!MeetsRequirement(requirement))
                {
                    return false;
                }
            }

            return true;
        }

        public void ResetAllSkills()
        {
            xpBySkill.Clear();
            AllSkillsReset?.Invoke();
        }

        public void AddXp(SkillType skill, float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            if (skillTree != null && !skillTree.IsUnlocked(skill))
            {
                return;
            }

            int oldLevel = GetLevel(skill);
            xpBySkill[skill] = GetXp(skill) + amount;
            int newLevel = GetLevel(skill);

            SkillXpChanged?.Invoke(skill, xpBySkill[skill], newLevel);

            if (newLevel > oldLevel)
            {
                SkillLeveledUp?.Invoke(skill, newLevel);
            }
        }
    }
}
