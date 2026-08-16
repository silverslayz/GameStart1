using UnityEngine;
using UnityEngine.UI;
using GameStart.UI;

namespace GameStart.Skills
{
    public class SkillSheetRow : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private StatusBar progressBar;
        [SerializeField] private SkillType skill;

        public SkillType Skill => skill;

        public void Initialize(SkillType skillType)
        {
            skill = skillType;
            if (nameText != null)
            {
                nameText.text = skill.ToString();
            }
        }

        public void SetLocked()
        {
            if (levelText != null)
            {
                levelText.text = "Locked";
            }

            if (progressBar != null)
            {
                progressBar.SetValue(0f, 1f);
            }
        }

        public void SetUnlocked(int level, float progressToNextLevel)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv {level}";
            }

            if (progressBar != null)
            {
                progressBar.SetValue(progressToNextLevel, 1f);
            }
        }
    }
}
