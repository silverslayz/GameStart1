using System;
using UnityEngine;
using GameStart.Player;
using GameStart.Skills;
using GameStart.UI;

namespace GameStart.Dungeons
{
    public class ApexBoss : MonoBehaviour
    {
        [SerializeField] private string bossName = "Apex Boss";
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private PlayerDungeonProgress dungeonProgress;
        [SerializeField] private PlayerSkills playerSkills;
        [SerializeField] private VictorySequenceUI victorySequence;

        [Header("Retaliation")]
        [SerializeField] private PlayerHealth targetPlayer;
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private float attackRange = 4f;

        public event Action<float, float> HealthChanged;
        public event Action BossDefeated;

        public string BossName => bossName;
        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDefeated { get; private set; }

        private float lastAttackTime = float.NegativeInfinity;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void OnEnable()
        {
            if (dungeonProgress != null)
            {
                dungeonProgress.DungeonEntered += OnDungeonEntered;
            }
        }

        private void OnDisable()
        {
            if (dungeonProgress != null)
            {
                dungeonProgress.DungeonEntered -= OnDungeonEntered;
            }
        }

        private void OnDungeonEntered(int dungeonIndex)
        {
            int combatLevel = playerSkills != null ? playerSkills.GetLevel(SkillType.Combat) : 1;
            ScaleForEncounter(dungeonIndex, combatLevel);
        }

        public void ScaleForEncounter(int dungeonIndex, int playerCombatLevel)
        {
            maxHealth = DifficultyScaling.GetBossMaxHealth(dungeonIndex, playerCombatLevel);
            attackDamage = DifficultyScaling.GetBossAttackDamage(dungeonIndex, playerCombatLevel);
            CurrentHealth = maxHealth;
            IsDefeated = false;

            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Update()
        {
            if (IsDefeated || targetPlayer == null || targetPlayer.IsDead)
            {
                return;
            }

            if (Time.time - lastAttackTime < attackInterval)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
            if (distance <= attackRange)
            {
                lastAttackTime = Time.time;
                targetPlayer.TakeDamage(attackDamage);
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDefeated || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
            {
                Defeat();
            }
        }

        private void Defeat()
        {
            IsDefeated = true;
            BossDefeated?.Invoke();

            if (dungeonProgress != null)
            {
                dungeonProgress.ClearCurrentDungeon();
            }

            if (victorySequence != null)
            {
                victorySequence.Show($"Apex Boss Defeated!\n{bossName}");
            }
        }
    }
}
