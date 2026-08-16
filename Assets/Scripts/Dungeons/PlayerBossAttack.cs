using UnityEngine;
using UnityEngine.InputSystem;
using GameStart.Skills;

namespace GameStart.Dungeons
{
    [RequireComponent(typeof(PlayerSkills))]
    public class PlayerBossAttack : MonoBehaviour
    {
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float baseDamage = 5f;
        [SerializeField] private float damagePerCombatLevel = 1f;
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

            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange * 0.5f);
            foreach (Collider hit in hits)
            {
                var boss = hit.GetComponent<ApexBoss>();
                if (boss != null)
                {
                    float damage = baseDamage + damagePerCombatLevel * skills.GetLevel(SkillType.Combat);
                    boss.TakeDamage(damage);
                    break;
                }
            }
        }
    }
}
