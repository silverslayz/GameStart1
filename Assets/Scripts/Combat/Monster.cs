using System;
using System.Collections;
using UnityEngine;
using GameStart.Economy;
using GameStart.Gathering;
using GameStart.UI;
using GameStart.Audio;
using GameStart.Flow;

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
        [SerializeField] private string questGemObjective = "Collect F-rank gems from monsters near the starting town";

        [SerializeField] private PlayerCurrency playerCurrency;
        [SerializeField] private PlayerResources playerResources;
        [SerializeField] private QuestLog questLog;
        [SerializeField] private PlayerBestiary bestiary;

        public event Action<float, float> HealthChanged;
        public event Action Defeated;

        public float CurrentHealth { get; private set; }
        public bool IsDefeated { get; private set; }
        public string BestiaryId => monsterId;

        private void Awake()
        {
            CurrentHealth = maxHealth;

            // Every one of these lives on the player, which a prefab can't reference. A
            // spawned monster resolves them itself so its drops and quest/bestiary hooks work.
            playerCurrency = SceneLink.Resolve(playerCurrency);
            playerResources = SceneLink.Resolve(playerResources);
            questLog = SceneLink.Resolve(questLog);
            bestiary = SceneLink.Resolve(bestiary);
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
            StartCoroutine(RespawnAfterDelay());
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
