using UnityEngine;
using UnityEngine.InputSystem;
using GameStart.Skills;
using GameStart.Audio;

namespace GameStart.Combat
{
    [RequireComponent(typeof(PlayerSkills))]
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float baseDamage = 5f;
        [SerializeField] private float damagePerCombatLevel = 1f;
        [SerializeField] private float swingCooldown = 0.5f;
        [SerializeField] private float combatXpPerSwing = 4f;
        [SerializeField] private PlayerBestiary bestiary;

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
            SfxPlayer.Play(SfxLibrary.AttackSwing);

            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange * 0.5f);
            foreach (Collider hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    float damage = baseDamage + damagePerCombatLevel * skills.GetLevel(SkillType.Combat);

                    var bestiaryTarget = hit.GetComponent<IBestiaryTarget>();
                    if (bestiaryTarget != null && bestiary != null)
                    {
                        damage *= bestiary.GetDamageMultiplier(bestiaryTarget.BestiaryId);
                    }

                    damageable.TakeDamage(damage);
                    break;
                }
            }
        }
    }
}
