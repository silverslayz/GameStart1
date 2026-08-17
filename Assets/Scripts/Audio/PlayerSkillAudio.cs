using UnityEngine;
using GameStart.Skills;

namespace GameStart.Audio
{
    [RequireComponent(typeof(PlayerSkills))]
    public class PlayerSkillAudio : MonoBehaviour
    {
        private PlayerSkills skills;

        private void Awake()
        {
            skills = GetComponent<PlayerSkills>();
        }

        private void OnEnable()
        {
            skills.SkillLeveledUp += OnLeveledUp;
        }

        private void OnDisable()
        {
            skills.SkillLeveledUp -= OnLeveledUp;
        }

        private void OnLeveledUp(SkillType skill, int newLevel)
        {
            SfxPlayer.Play(SfxLibrary.LevelUp);
        }
    }
}
