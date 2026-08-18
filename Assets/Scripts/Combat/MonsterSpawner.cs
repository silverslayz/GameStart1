using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GameStart.Combat
{
    /// <summary>
    /// Populates an area with monsters and keeps it populated (#203).
    ///
    /// Before this the scene held a single hand-placed monster that revived in place, so
    /// encounter density was scene layout rather than data, and a hundred dungeons would
    /// have meant a hundred hand-populated scenes.
    ///
    /// The spawner owns its monsters' lifecycle: it clears their SelfRespawn so they stay
    /// down when killed, and decides when and where a replacement appears.
    /// </summary>
    public class MonsterSpawner : MonoBehaviour
    {
        [Header("What to spawn")]
        [SerializeField] private GameObject monsterPrefab;

        [Tooltip("Leave at 0 to take the count from SpawnBudget using the dungeon index below.")]
        [SerializeField] private int maxAlive = 0;

        [Tooltip("Used only when maxAlive is 0, to look the count up per dungeon tier.")]
        [SerializeField] private int dungeonIndex = 1;

        [Header("Where")]
        [SerializeField] private float spawnRadius = 8f;

        [Tooltip("How far from a sampled point the NavMesh may be for the spot to count as valid.")]
        [SerializeField] private float navMeshSnapDistance = 3f;

        [Header("When")]
        [SerializeField] private float respawnDelay = 12f;
        [SerializeField] private bool spawnOnStart = true;

        [Tooltip("Skip spawning while the player is further away than this. 0 disables the check.")]
        [SerializeField] private float activationDistance = 0f;

        private readonly List<Monster> alive = new List<Monster>();
        private Transform player;
        private bool warnedNoPrefab;

        public int AliveCount => alive.Count;
        public int Capacity => maxAlive > 0 ? maxAlive : SpawnBudget.GetMonsterCount(dungeonIndex);

        private void Start()
        {
            if (spawnOnStart)
            {
                FillToCapacity();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe so a disabled spawner cannot resurrect anything.
            foreach (var m in alive)
            {
                if (m != null) m.Defeated -= OnAnyDefeated;
            }
        }

        /// <summary>Spawns until the area is at capacity. Safe to call repeatedly.</summary>
        public void FillToCapacity()
        {
            alive.RemoveAll(m => m == null);

            int target = Capacity;
            for (int i = alive.Count; i < target; i++)
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            if (monsterPrefab == null)
            {
                if (!warnedNoPrefab)
                {
                    warnedNoPrefab = true;
                    Debug.LogWarning($"{name}: no monster prefab assigned, so nothing will spawn.", this);
                }
                return;
            }

            if (!TryFindSpawnPoint(out Vector3 point))
            {
                // No NavMesh nearby: spawning anyway would produce an inert monster that
                // can never path, which looks like broken AI rather than bad placement.
                Debug.LogWarning($"{name}: no NavMesh within {spawnRadius}m, skipping spawn.", this);
                return;
            }

            var go = Instantiate(monsterPrefab, point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            go.name = monsterPrefab.name + "_Spawned";

            var monster = go.GetComponent<Monster>();
            if (monster == null)
            {
                Debug.LogWarning($"{name}: prefab has no Monster component.", this);
                Destroy(go);
                return;
            }

            // Scale to the tier this spawner belongs to. The spawner is the only thing that
            // knows which dungeon a monster came from, which is why scaling lives here
            // rather than on the prefab.
            int combatLevel = GetPlayerCombatLevel();
            monster.ScaleForEncounter(dungeonIndex, combatLevel);

            var attack = go.GetComponent<MonsterAttack>();
            if (attack != null)
            {
                attack.ScaleForEncounter(dungeonIndex, combatLevel);
            }

            // This spawner owns repopulation from here.
            monster.SelfRespawn = false;
            monster.Defeated += OnAnyDefeated;
            alive.Add(monster);

            // Each monster leashes to its OWN spawn point, not to the spawner's centre.
            // Sharing one home made all three walk to the same spot and stack on top of
            // each other the moment they spawned - they were placed spread out and then
            // converged. The spawn radius is what keeps the group together.
            var pursuit = go.GetComponent<MonsterPursuit>();
            if (pursuit != null)
            {
                pursuit.SetHome(point);
            }
        }

        private bool TryFindSpawnPoint(out Vector3 point)
        {
            for (int attempt = 0; attempt < 12; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * spawnRadius;
                Vector3 candidate = transform.position + new Vector3(offset.x, 0f, offset.y);

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSnapDistance, NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = transform.position;
            return false;
        }

        private void OnAnyDefeated()
        {
            StartCoroutine(ReplaceAfterDelay());
        }

        private IEnumerator ReplaceAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);

            // Clear out corpses before topping up, otherwise capacity is counted against
            // monsters that are already dead.
            for (int i = alive.Count - 1; i >= 0; i--)
            {
                Monster m = alive[i];
                if (m == null)
                {
                    alive.RemoveAt(i);
                    continue;
                }

                if (m.IsDefeated)
                {
                    m.Defeated -= OnAnyDefeated;
                    alive.RemoveAt(i);
                    Destroy(m.gameObject);
                }
            }

            if (activationDistance > 0f && !IsPlayerNear())
            {
                yield break;
            }

            FillToCapacity();
        }

        private int GetPlayerCombatLevel()
        {
            var skills = FindAnyObjectByType<GameStart.Skills.PlayerSkills>();
            return skills != null ? skills.GetLevel(GameStart.Skills.SkillType.Combat) : 1;
        }

        private bool IsPlayerNear()
        {
            if (player == null)
            {
                var health = FindAnyObjectByType<GameStart.Player.PlayerHealth>();
                if (health == null) return true;   // no player yet; don't block spawning
                player = health.transform;
            }

            return Vector3.Distance(player.position, transform.position) <= activationDistance;
        }

        /// <summary>Removes every monster this spawner created, for leaving or resetting a dungeon.</summary>
        public void DespawnAll()
        {
            foreach (var m in alive)
            {
                if (m == null) continue;
                m.Defeated -= OnAnyDefeated;
                Destroy(m.gameObject);
            }

            alive.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.3f, 0.3f, 0.45f);
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
