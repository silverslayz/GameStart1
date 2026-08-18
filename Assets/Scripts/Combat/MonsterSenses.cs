using System;
using UnityEngine;
using GameStart.Player;
using GameStart.Flow;

namespace GameStart.Combat
{
    /// <summary>
    /// Decides whether a monster has noticed the player (#177).
    ///
    /// Kept separate from pursuit and attack so each can be tuned independently and so
    /// other systems - audio stings, UI "noticed you" cues - can react to aggro without
    /// reaching into movement code.
    /// </summary>
    [RequireComponent(typeof(Monster))]
    public class MonsterSenses : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRadius = 12f;

        [Tooltip("Inside this radius the monster notices the player regardless of facing - " +
                 "walking up behind it should still get a reaction.")]
        [SerializeField] private float alwaysNoticeRadius = 3.5f;

        [Tooltip("When true, detection outside the always-notice radius requires the player " +
                 "to be within the vision cone.")]
        [SerializeField] private bool useVisionCone = true;
        [SerializeField, Range(10f, 360f)] private float visionConeAngle = 140f;

        [Header("Losing the player")]
        [Tooltip("Aggro persists this far beyond the detection radius, so a monster doesn't " +
                 "flicker in and out of combat at the boundary.")]
        [SerializeField] private float loseAggroPadding = 5f;

        /// <summary>Raised when aggro is gained or lost. Consumers avoid polling.</summary>
        public event Action<bool> AggroChanged;

        public bool IsAggroed { get; private set; }
        public PlayerHealth Target { get; private set; }
        public float DetectionRadius
        {
            get => detectionRadius;
            set => detectionRadius = value;
        }

        private Monster monster;
        private PlayerHealth player;

        private void Awake()
        {
            monster = GetComponent<Monster>();
            // Single player for now. Multi-target selection waits on multiplayer sessions
            // (#37-40); this is the one place that choice needs to change.
            player = SceneLink.Resolve<PlayerHealth>(null);
        }

        private void Update()
        {
            if (player == null)
            {
                player = SceneLink.Resolve<PlayerHealth>(null);
                if (player == null) return;
            }

            // A defeated monster stops noticing anything; respawn re-arms it.
            bool eligible = monster != null && !monster.IsDefeated && !player.IsDead;
            SetAggro(eligible && CanSensePlayer());
        }

        private bool CanSensePlayer()
        {
            Vector3 toPlayer = player.transform.position - transform.position;
            float distance = toPlayer.magnitude;

            // Hysteresis: a wider radius to lose aggro than to gain it.
            float threshold = IsAggroed ? detectionRadius + loseAggroPadding : detectionRadius;
            if (distance > threshold)
            {
                return false;
            }

            if (distance <= alwaysNoticeRadius || !useVisionCone)
            {
                return true;
            }

            // Once aggroed the monster keeps tracking even if the player leaves the cone -
            // otherwise it forgets the player the moment it turns to path around an obstacle.
            if (IsAggroed)
            {
                return true;
            }

            Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flat.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            float angle = Vector3.Angle(transform.forward, flat.normalized);
            return angle <= visionConeAngle * 0.5f;
        }

        private void SetAggro(bool value)
        {
            if (IsAggroed == value)
            {
                return;
            }

            IsAggroed = value;
            Target = value ? player : null;
            AggroChanged?.Invoke(value);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, alwaysNoticeRadius);
        }
    }
}
