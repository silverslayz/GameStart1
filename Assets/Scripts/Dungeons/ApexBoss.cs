using System;
using UnityEngine;
using GameStart.Combat;
using GameStart.Player;
using GameStart.Skills;
using GameStart.UI;
using GameStart.Audio;
using GameStart.Narrative;

namespace GameStart.Dungeons
{
    public class ApexBoss : MonoBehaviour, IDamageable, IBestiaryTarget
    {
        [SerializeField] private string bossName = "Apex Boss";
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private PlayerDungeonProgress dungeonProgress;
        [SerializeField] private PlayerSkills playerSkills;
        [SerializeField] private VictorySequenceUI victorySequence;
        [SerializeField] private PlayerBestiary bestiary;

        public string BestiaryId => bossName;

        [Header("Retaliation")]
        [SerializeField] private PlayerHealth targetPlayer;
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private float attackRange = 4f;

        [Header("Bestiary")]
        [Tooltip("Damage multiplier applied to this boss's retaliation once the player has analyzed its weakness - a clearer, more readable tell.")]
        [SerializeField] private float analyzedAttackDamageMultiplier = 0.75f;

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

                float damage = attackDamage;
                if (bestiary != null && bestiary.IsWeaknessDiscovered(bossName))
                {
                    damage *= analyzedAttackDamageMultiplier;
                }

                targetPlayer.TakeDamage(damage);
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
            SfxPlayer.Play(SfxLibrary.DamageHit);

            if (CurrentHealth <= 0f)
            {
                Defeat();
            }
        }

        private void Defeat()
        {
            IsDefeated = true;
            BossDefeated?.Invoke();
            SfxPlayer.Play(SfxLibrary.MonsterDefeat);
            bestiary?.RecordKill(bossName);

            // Capture the biome before clearing - ClearCurrentDungeon() advances
            // the index, which would otherwise point at the *next* dungeon's biome.
            string biome = dungeonProgress != null ? dungeonProgress.CurrentDungeon.Biome : null;

            if (dungeonProgress != null)
            {
                dungeonProgress.ClearCurrentDungeon();
            }

            if (victorySequence != null)
            {
                string message = $"Apex Boss Defeated!\n{bossName}";
                if (!string.IsNullOrEmpty(biome))
                {
                    LoreEntry bossLore = LoreLibrary.GetBossLore(biome);
                    message += $"\n\n{bossLore.Body}";
                }

                victorySequence.Show(message);
            }
        }
    }
}
