using UnityEngine;

namespace GameStart.Combat
{
    /// <summary>
    /// Stamps an archetype's behaviour onto a monster's senses, pursuit and attack (#179).
    ///
    /// Applied in Start rather than Awake, because MonsterPursuit creates its NavMeshAgent
    /// in its own Awake and the profile has to reach that agent. Start runs after every
    /// Awake, so the components are guaranteed to exist.
    ///
    /// It writes only behaviour - speed, ranges, leash, attack timing. Health and damage
    /// magnitudes belong to the archetype stat work (#108), and difficulty scaling
    /// multiplies whatever those end up being (#204).
    /// </summary>
    [RequireComponent(typeof(Monster))]
    [RequireComponent(typeof(MonsterSenses))]
    public class MonsterArchetypeBinding : MonoBehaviour
    {
        [SerializeField] private MonsterArchetype archetype = MonsterArchetype.Brute;

        public MonsterArchetype Archetype => archetype;

        private void Start()
        {
            Apply(archetype);
        }

        /// <summary>Re-stamps the monster with a different archetype, e.g. from spawner data.</summary>
        public void Apply(MonsterArchetype value)
        {
            archetype = value;
            MonsterArchetypeProfile profile = MonsterArchetypeCatalog.Get(value);

            var senses = GetComponent<MonsterSenses>();
            if (senses != null)
            {
                senses.DetectionRadius = profile.DetectionRadius;
            }

            var pursuit = GetComponent<MonsterPursuit>();
            if (pursuit != null)
            {
                pursuit.ApplyProfile(profile);
            }

            var attack = GetComponent<MonsterAttack>();
            if (attack != null)
            {
                // Damage is left as authored: Configure would otherwise overwrite the value
                // #204's scaling multiplies, and #108 owns the magnitude anyway.
                attack.ConfigureBehaviour(profile.Style, profile.AttackRange, profile.Windup, profile.Cooldown);
            }
        }
    }
}
