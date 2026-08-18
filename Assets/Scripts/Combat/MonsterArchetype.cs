using System.Collections.Generic;

namespace GameStart.Combat
{
    public enum MonsterArchetype
    {
        /// <summary>Baseline melee chaser. What every monster behaved like before this.</summary>
        Brute,

        /// <summary>Keeps its distance and backs off when closed on.</summary>
        Ranged,

        /// <summary>Slow, stubborn, hard to pull off its post.</summary>
        Tanky,

        /// <summary>Quick, circles rather than charging straight in.</summary>
        Fast
    }

    /// <summary>
    /// Behaviour parameters per archetype.
    ///
    /// Deliberately carries no health or damage magnitudes - those belong to the archetype
    /// stat work in #108. This is the behaviour half: how a monster moves, how close it
    /// wants to be, how stubbornly it holds its ground, and how it times a swing. Keeping
    /// the split means the two can land independently without fighting over the same fields.
    /// </summary>
    public struct MonsterArchetypeProfile
    {
        public float DetectionRadius;

        public float MoveSpeed;
        public float DesiredRange;      // where it settles relative to its target
        public float RetreatRange;      // closer than this and it backs away; 0 disables
        public float FlankStrength;     // 0 charges straight, 1 circles hard
        public float MaxLeashDistance;
        public float GiveUpDelay;

        public MonsterAttackStyle Style;
        public float AttackRange;
        public float Windup;
        public float Cooldown;
    }

    public static class MonsterArchetypeCatalog
    {
        private static readonly Dictionary<MonsterArchetype, MonsterArchetypeProfile> Profiles =
            new Dictionary<MonsterArchetype, MonsterArchetypeProfile>
        {
            {
                MonsterArchetype.Brute, new MonsterArchetypeProfile
                {
                    DetectionRadius = 12f,
                    MoveSpeed = 3.2f, DesiredRange = 1.8f, RetreatRange = 0f, FlankStrength = 0f,
                    MaxLeashDistance = 22f, GiveUpDelay = 3f,
                    Style = MonsterAttackStyle.Melee, AttackRange = 2.2f, Windup = 0.55f, Cooldown = 2.0f,
                }
            },
            {
                // Hangs back and kites. Its attack range is long enough that DesiredRange
                // still leaves it able to shoot without closing.
                MonsterArchetype.Ranged, new MonsterArchetypeProfile
                {
                    DetectionRadius = 15f,
                    MoveSpeed = 3.0f, DesiredRange = 8f, RetreatRange = 5.5f, FlankStrength = 0.2f,
                    MaxLeashDistance = 20f, GiveUpDelay = 4f,
                    Style = MonsterAttackStyle.Ranged, AttackRange = 12f, Windup = 0.9f, Cooldown = 2.8f,
                }
            },
            {
                // Slow but stubborn: a short leash and a long give-up mean it holds its post
                // rather than being kited away from whatever it is guarding.
                MonsterArchetype.Tanky, new MonsterArchetypeProfile
                {
                    DetectionRadius = 10f,
                    MoveSpeed = 2.0f, DesiredRange = 1.9f, RetreatRange = 0f, FlankStrength = 0f,
                    MaxLeashDistance = 12f, GiveUpDelay = 5f,
                    Style = MonsterAttackStyle.Melee, AttackRange = 2.4f, Windup = 0.9f, Cooldown = 2.6f,
                }
            },
            {
                // Fast and evasive: circles the target instead of running down the same line,
                // which also stops a pack of them stacking into one column.
                MonsterArchetype.Fast, new MonsterArchetypeProfile
                {
                    DetectionRadius = 14f,
                    MoveSpeed = 5.2f, DesiredRange = 1.6f, RetreatRange = 0f, FlankStrength = 0.85f,
                    MaxLeashDistance = 26f, GiveUpDelay = 2f,
                    Style = MonsterAttackStyle.Melee, AttackRange = 2.0f, Windup = 0.35f, Cooldown = 1.3f,
                }
            },
        };

        public static MonsterArchetypeProfile Get(MonsterArchetype archetype)
        {
            return Profiles.TryGetValue(archetype, out MonsterArchetypeProfile profile)
                ? profile
                : Profiles[MonsterArchetype.Brute];
        }
    }
}
