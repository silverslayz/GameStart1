using System;
using System.Collections.Generic;
using UnityEngine;
using GameStart.Skills;

namespace GameStart.Class
{
    public class PlayerSkillTree : MonoBehaviour
    {
        private readonly HashSet<SkillType> unlockedSkills = new HashSet<SkillType>();

        public event Action<IReadOnlyCollection<SkillType>> SkillTreeLocked;

        public bool IsLocked { get; private set; }
        public IReadOnlyCollection<SkillType> UnlockedSkills => unlockedSkills;

        public bool IsUnlocked(SkillType skill) => unlockedSkills.Contains(skill);

        private PlayerClassSelection classSelection;

        private void Awake()
        {
            classSelection = GetComponent<PlayerClassSelection>();
        }

        private void OnEnable()
        {
            if (classSelection != null)
            {
                classSelection.ClassSelected += LockInBaseTree;
            }
        }

        private void OnDisable()
        {
            if (classSelection != null)
            {
                classSelection.ClassSelected -= LockInBaseTree;
            }
        }

        private void LockInBaseTree(PlayerClassType classType)
        {
            if (IsLocked)
            {
                return;
            }

            foreach (SkillType skill in ClassSkillTreeCatalog.GetBaseTree(classType))
            {
                unlockedSkills.Add(skill);
            }

            IsLocked = true;
            SkillTreeLocked?.Invoke(unlockedSkills);
        }
    }
}
