using System;
using System.Collections;
using UnityEngine;
using GameStart.Economy;
using GameStart.Gathering;
using GameStart.UI;
using GameStart.Audio;
using GameStart.Flow;
using GameStart.Dungeons;

namespace GameStart.Combat
{
    public class Monster : MonoBehaviour, IDamageable, IBestiaryTarget
    {
        [SerializeField] private string monsterId = "Grunt";
        [SerializeField] private float maxHealth = 30f;
        [SerializeField] private int minGemDrop = 1;
        [SerializeField] private int maxGemDrop = 3;
        [SerializeField] private string rawFoodResourceName = "Raw Meat";
        [SerializeField] private int minFoodDrop = 1;
        [SerializeField] private int maxFoodDrop = 2;
        [SerializeField] private float respawnTime = 20f;

        [Tooltip("Off when a spawner owns this monster's lifecycle: it stays defeated and the "
                 + "spawner decides when and where a replacement appears.")]
        [SerializeField] private bool selfRespawn = true;
        [SerializeField] private string questGemObjective = "Collect F-rank gems from monsters near the starting town";

        [SerializeField] private PlayerCurrency playerCurrency;
        [SerializeField] private PlayerResources playerResources;
        [SerializeField] private QuestLog questLog;
        [SerializeField] private PlayerBestiary bestiary;

        public event Action<float, float> HealthChanged;
        public event Action Defeated;

        public float CurrentHealth { get; private set; }
        public bool IsDefeated { get; private set; }

        /// <summary>Spawners clear this so they, not the monster, control repopulation.</summary>
        public bool SelfRespawn
        {
            get => selfRespawn;
            set => selfRespawn = value;
        }
        public string BestiaryId => monsterId;

        // Authored value, remembered so repeated scaling multiplies the prefab's number
        // rather than compounding on the last result.
        private float baseMaxHealth;

        private void Awake()
        {
            baseMaxHealth = maxHealth;
            CurrentHealth = maxHealth;

            // Every one of these lives on the player, which a prefab can't reference. A
            // spawned monster resolves them itself so its drops and quest/bestiary hooks work.
            playerCurrency = SceneLink.Resolve(playerCurrency);
            playerResources = SceneLink.Resolve(playerResources);
            questLog = SceneLink.Resolve(questLog);
            bestiary = SceneLink.Resolve(bestiary);
        }

        /// <summary>
        /// Scales this monster for the dungeon it belongs to. Multiplies the authored max
        /// health rather than replacing it, so archetype differences survive.
        /// </summary>
        public void ScaleForEncounter(int dungeonIndex, int playerCombatLevel)
        {
            if (baseMaxHealth <= 0f)
            {
                baseMaxHealth = maxHealth;
            }

            maxHealth = baseMaxHealth * DifficultyScaling.GetMonsterHealthMultiplier(dungeonIndex, playerCombatLevel);
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
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
            FloatingCombatText.ShowDamage(transform, amount);

            if (CurrentHealth <= 0f)
            {
                Defeat();
            }
        }

        private void Defeat()
        {
            IsDefeated = true;
            Defeated?.Invoke();
            SfxPlayer.Play(SfxLibrary.MonsterDefeat);
            bestiary?.RecordKill(monsterId);

            int gemAmount = UnityEngine.Random.Range(minGemDrop, maxGemDrop + 1);
            playerCurrency?.AddGems(gemAmount);

            if (questLog != null)
            {
                int index = questLog.FindObjectiveIndex(questGemObjective);
                if (index >= 0)
                {
                    questLog.AddProgress(index, gemAmount);
                }
            }

            int foodAmount = UnityEngine.Random.Range(minFoodDrop, maxFoodDrop + 1);
            playerResources?.AddResource(rawFoodResourceName, foodAmount);

            SetPresenceActive(false);

            // A spawner-owned monster stays down; the spawner replaces it. Respawning
            // here as well would leave two monsters where the budget allowed one.
            if (selfRespawn)
            {
                StartCoroutine(RespawnAfterDelay());
            }
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnTime);
            CurrentHealth = maxHealth;
            IsDefeated = false;
            SetPresenceActive(true);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void SetPresenceActive(bool active)
        {
            // Disable visuals/collision rather than the whole GameObject, so this
            // script's own respawn coroutine keeps running while "dead".
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = active;
            }

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = active;
            }
        }
    }
}
