using System;
using System.Collections;
using UnityEngine;
using GameStart.Audio;

namespace GameStart.Combat
{
    public enum MonsterAttackStyle
    {
        Melee,
        Ranged
    }

    /// <summary>
    /// Winds up, then applies damage to the player (#178).
    ///
    /// The windup is the point: an instant hit is unreadable and impossible to react to.
    /// This mirrors the telegraph approach already used by ApexBoss (#134) so both read
    /// the same way to a player.
    /// </summary>
    [RequireComponent(typeof(Monster))]
    [RequireComponent(typeof(MonsterSenses))]
    public class MonsterAttack : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private MonsterAttackStyle style = MonsterAttackStyle.Melee;
        [SerializeField] private float attackDamage = 6f;

        [Tooltip("Shorter than the aggro radius - this is the range at which it commits to a swing.")]
        [SerializeField] private float attackRange = 2.2f;

        [Tooltip("Telegraph before the hit lands, so the player can react.")]
        [SerializeField] private float windup = 0.55f;

        [Tooltip("Minimum seconds between attacks; stops machine-gun repeats.")]
        [SerializeField] private float cooldown = 2.0f;

        [Tooltip("Ranged attacks resolve as hitscan for now; a projectile can replace this " +
                 "without changing the timing contract.")]
        [SerializeField] private float rangedMaxRange = 12f;

        /// <summary>Raised when a swing starts, so VFX/audio can telegraph it (#181).</summary>
        public event Action WindupStarted;
        public event Action<float> DamageDealt;

        public MonsterAttackStyle Style
        {
            get => style;
            set => style = value;
        }
        public float AttackRange => style == MonsterAttackStyle.Ranged ? rangedMaxRange : attackRange;
        public bool IsWindingUp { get; private set; }

        private Monster monster;
        private MonsterSenses senses;
        private float lastAttackTime = float.NegativeInfinity;

        private void Awake()
        {
            monster = GetComponent<Monster>();
            senses = GetComponent<MonsterSenses>();
        }

        private void Update()
        {
            if (IsWindingUp || monster == null || monster.IsDefeated) return;
            if (senses == null || !senses.IsAggroed || senses.Target == null) return;
            if (Time.time - lastAttackTime < cooldown) return;

            float distance = Vector3.Distance(transform.position, senses.Target.transform.position);
            if (distance <= AttackRange)
            {
                StartCoroutine(Swing());
            }
        }

        private IEnumerator Swing()
        {
            IsWindingUp = true;
            lastAttackTime = Time.time;
            WindupStarted?.Invoke();
            SfxPlayer.Play(SfxLibrary.DamageHit);

            yield return new WaitForSeconds(windup);

            IsWindingUp = false;

            // Re-checked after the windup: stepping out of range during the telegraph is
            // exactly how a player is meant to avoid the hit.
            if (monster.IsDefeated || senses.Target == null || senses.Target.IsDead)
            {
                yield break;
            }

            float distance = Vector3.Distance(transform.position, senses.Target.transform.position);
            float allowed = AttackRange + 0.4f;   // small grace so a hit isn't lost to jitter
            if (distance > allowed)
            {
                yield break;
            }

            senses.Target.TakeDamage(attackDamage);
            DamageDealt?.Invoke(attackDamage);
        }

        /// <summary>Lets archetype data drive the numbers without exposing the fields.</summary>
        public void Configure(MonsterAttackStyle attackStyle, float damage, float range, float windupSeconds, float cooldownSeconds)
        {
            style = attackStyle;
            attackDamage = damage;
            if (attackStyle == MonsterAttackStyle.Ranged) rangedMaxRange = range;
            else attackRange = range;
            windup = windupSeconds;
            cooldown = cooldownSeconds;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.2f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}
