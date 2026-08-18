using UnityEngine;
using UnityEngine.AI;

namespace GameStart.Combat
{
    /// <summary>
    /// Moves an aggroed monster toward its target, and returns it home when it loses
    /// interest (#176).
    ///
    /// The leash is the important half: without it a monster aggroed at a dungeon entrance
    /// follows the player forever, and a run turns into a train of every enemy on the floor.
    /// </summary>
    [RequireComponent(typeof(Monster))]
    [RequireComponent(typeof(MonsterSenses))]
    public class MonsterPursuit : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float angularSpeed = 260f;
        [SerializeField] private float acceleration = 12f;

        [Tooltip("Stop this far from the target. Should sit just inside the attack range so " +
                 "the monster doesn't jitter between stopping and closing.")]
        [SerializeField] private float stoppingDistance = 1.8f;

        [Header("Leash")]
        [Tooltip("How far the monster may stray from where it started before giving up.")]
        [SerializeField] private float maxLeashDistance = 22f;

        [Tooltip("Seconds without aggro before it walks home.")]
        [SerializeField] private float giveUpDelay = 3f;

        [Header("Archetype behaviour")]
        [Tooltip("Where this monster settles relative to its target. Ranged types sit well back.")]
        [SerializeField] private float desiredRange = 1.8f;

        [Tooltip("Closer than this and it backs away to desiredRange. 0 disables kiting.")]
        [SerializeField] private float retreatRange = 0f;

        [Tooltip("0 charges straight at the target, 1 circles hard around it.")]
        [SerializeField, Range(0f, 1f)] private float flankStrength = 0f;

        public bool IsReturningHome { get; private set; }
        public float MoveSpeed
        {
            get => moveSpeed;
            set
            {
                moveSpeed = value;
                if (agent != null) agent.speed = value;
            }
        }

        private Monster monster;
        private MonsterSenses senses;
        private NavMeshAgent agent;

        private Vector3 homePosition;
        private float lostTargetAt = float.NegativeInfinity;
        private bool warnedOffMesh;

        // Chosen once per monster so a pack circles both ways instead of forming a
        // single conga line around the player.
        private float flankSign = 1f;

        private void Awake()
        {
            monster = GetComponent<Monster>();
            senses = GetComponent<MonsterSenses>();
            homePosition = transform.position;

            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }

            agent.speed = moveSpeed;
            agent.angularSpeed = angularSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = stoppingDistance;
            // The mesh is authored around the origin at the monster's feet.
            agent.baseOffset = 0f;

            flankSign = Random.value < 0.5f ? -1f : 1f;
        }

        private void OnEnable()
        {
            if (senses != null) senses.AggroChanged += OnAggroChanged;
        }

        private void OnDisable()
        {
            if (senses != null) senses.AggroChanged -= OnAggroChanged;
        }

        private void OnAggroChanged(bool aggroed)
        {
            if (!aggroed)
            {
                lostTargetAt = Time.time;
            }
            else
            {
                IsReturningHome = false;
            }
        }

        private void Update()
        {
            if (agent == null)
            {
                return;
            }

            if (!agent.isOnNavMesh)
            {
                // Warn once rather than every frame: without a baked NavMesh the agent is
                // inert, and silence here looks like broken AI rather than missing setup.
                if (!warnedOffMesh)
                {
                    warnedOffMesh = true;
                    Debug.LogWarning($"{name}: not on a NavMesh, so it cannot pursue. Bake one via the NavMeshSurface in the scene.", this);
                }
                return;
            }

            if (monster != null && monster.IsDefeated)
            {
                Stop();
                return;
            }

            bool leashed = Vector3.Distance(transform.position, homePosition) > maxLeashDistance;

            if (senses != null && senses.IsAggroed && senses.Target != null && !leashed)
            {
                IsReturningHome = false;
                agent.isStopped = false;
                agent.stoppingDistance = stoppingDistance;
                agent.SetDestination(ComputeChaseDestination(senses.Target.transform.position));
                return;
            }

            // Out of aggro, or dragged too far from home.
            bool waitedLongEnough = Time.time - lostTargetAt >= giveUpDelay;
            if (leashed || waitedLongEnough)
            {
                ReturnHome();
            }
        }

        /// <summary>
        /// Where the monster actually wants to stand, rather than simply the target's feet.
        /// This is what makes archetypes read differently: a ranged type retreats to keep
        /// its distance, a fast type arcs around instead of charging down the same line.
        /// </summary>
        private Vector3 ComputeChaseDestination(Vector3 targetPosition)
        {
            Vector3 fromTarget = transform.position - targetPosition;
            float distance = fromTarget.magnitude;
            if (distance < 0.01f)
            {
                return targetPosition;
            }

            Vector3 outward = fromTarget / distance;

            // Kiting: back off to the preferred range rather than standing and trading.
            if (retreatRange > 0f && distance < retreatRange)
            {
                return targetPosition + outward * Mathf.Max(desiredRange, retreatRange);
            }

            if (flankStrength > 0f)
            {
                Vector3 sideways = Vector3.Cross(Vector3.up, outward) * flankSign;
                Vector3 approach = (outward + sideways * flankStrength).normalized;
                return targetPosition + approach * Mathf.Max(desiredRange, 0.5f);
            }

            return targetPosition;
        }

        /// <summary>Applies an archetype's movement parameters.</summary>
        public void ApplyProfile(MonsterArchetypeProfile profile)
        {
            MoveSpeed = profile.MoveSpeed;
            desiredRange = profile.DesiredRange;
            retreatRange = profile.RetreatRange;
            flankStrength = profile.FlankStrength;
            maxLeashDistance = profile.MaxLeashDistance;
            giveUpDelay = profile.GiveUpDelay;

            stoppingDistance = Mathf.Max(0.5f, profile.DesiredRange - 0.2f);
            if (agent != null)
            {
                agent.stoppingDistance = stoppingDistance;
            }
        }

        private void ReturnHome()
        {
            float distanceHome = Vector3.Distance(transform.position, homePosition);
            if (distanceHome < 0.6f)
            {
                IsReturningHome = false;
                Stop();
                return;
            }

            IsReturningHome = true;
            agent.isStopped = false;
            agent.stoppingDistance = 0.1f;
            agent.SetDestination(homePosition);
        }

        private void Stop()
        {
            if (agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        /// <summary>Re-anchors the leash, for monsters placed or spawned after Awake.</summary>
        public void SetHome(Vector3 position)
        {
            homePosition = position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.5f);
            Vector3 home = Application.isPlaying ? homePosition : transform.position;
            Gizmos.DrawWireSphere(home, maxLeashDistance);
        }
    }
}
