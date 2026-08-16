using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.Skills
{
    [RequireComponent(typeof(PlayerSkills))]
    public class PlayerCombatAction : MonoBehaviour
    {
        [SerializeField] private float combatXpPerSwing = 4f;
        [SerializeField] private float swingCooldown = 0.5f;

        private PlayerSkills skills;
        private float lastSwingTime = float.NegativeInfinity;

        private void Awake()
        {
            skills = GetComponent<PlayerSkills>();
        }

        // Called automatically by PlayerInput (Behavior: Send Messages)
        public void OnAttack(InputValue value)
        {
            if (!value.isPressed)
            {
                return;
            }

            if (Time.time - lastSwingTime < swingCooldown)
            {
                return;
            }

            lastSwingTime = Time.time;
            skills.AddXp(SkillType.Combat, combatXpPerSwing);
        }
    }
}
