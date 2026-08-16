using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GameStart.Class;

namespace GameStart.Skills
{
    public class SkillSheetUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private PlayerSkills skills;
        [SerializeField] private PlayerSkillTree skillTree;
        [SerializeField] private List<SkillSheetRow> rows = new List<SkillSheetRow>();

        private bool isOpen;

        private void OnEnable()
        {
            if (skills != null)
            {
                skills.SkillXpChanged += OnSkillXpChanged;
            }

            if (skillTree != null)
            {
                skillTree.SkillTreeLocked += OnSkillTreeLocked;
            }
        }

        private void OnDisable()
        {
            if (skills != null)
            {
                skills.SkillXpChanged -= OnSkillXpChanged;
            }

            if (skillTree != null)
            {
                skillTree.SkillTreeLocked -= OnSkillTreeLocked;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            {
                SetOpen(!isOpen);
            }
        }

        private void SetOpen(bool open)
        {
            isOpen = open;
            if (panel != null)
            {
                panel.SetActive(open);
            }

            if (open)
            {
                RefreshAllRows();
            }
        }

        private void OnSkillTreeLocked(IReadOnlyCollection<SkillType> unlockedSkills)
        {
            RefreshAllRows();
        }

        private void OnSkillXpChanged(SkillType skill, float xp, int level)
        {
            RefreshRow(skill);
        }

        private void RefreshAllRows()
        {
            foreach (SkillSheetRow row in rows)
            {
                RefreshRow(row.Skill);
            }
        }

        private void RefreshRow(SkillType skill)
        {
            SkillSheetRow row = rows.Find(r => r.Skill == skill);
            if (row == null)
            {
                return;
            }

            if (skillTree != null && !skillTree.IsUnlocked(skill))
            {
                row.SetLocked();
                return;
            }

            int level = skills.GetLevel(skill);
            float progress = SkillLevelCurve.GetProgressToNextLevel(skills.GetXp(skill));
            row.SetUnlocked(level, progress);
        }
    }
}
