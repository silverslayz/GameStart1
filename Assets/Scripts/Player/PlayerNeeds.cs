using System;
using UnityEngine;
using GameStart.Skills;

namespace GameStart.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerNeeds : MonoBehaviour
    {
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float hungerDepletePerSecond = 0.5f;
        [SerializeField] private float thirstDepletePerSecond = 0.8f;
        [SerializeField] private float starvingDamagePerSecond = 2f;
        [SerializeField] private float survivalXpPerConsumeAmount = 0.5f;

        private PlayerHealth health;
        private PlayerSkills skills;

        public event Action<float, float> HungerChanged;
        public event Action<float, float> ThirstChanged;

        public float MaxHunger => maxHunger;
        public float MaxThirst => maxThirst;
        public float CurrentHunger { get; private set; }
        public float CurrentThirst { get; private set; }
        public bool IsStarving => CurrentHunger <= 0f || CurrentThirst <= 0f;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            skills = GetComponent<PlayerSkills>();
            CurrentHunger = maxHunger;
            CurrentThirst = maxThirst;
        }

        private void Update()
        {
            SetHunger(CurrentHunger - hungerDepletePerSecond * Time.deltaTime);
            SetThirst(CurrentThirst - thirstDepletePerSecond * Time.deltaTime);

            if (IsStarving && !health.IsDead)
            {
                health.TakeDamage(starvingDamagePerSecond * Time.deltaTime);
            }
        }

        public void Eat(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetHunger(CurrentHunger + amount);
            skills?.AddXp(SkillType.Survival, amount * survivalXpPerConsumeAmount);
        }

        public void RestoreFull()
        {
            SetHunger(maxHunger);
            SetThirst(maxThirst);
        }

        public void Drink(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetThirst(CurrentThirst + amount);
            skills?.AddXp(SkillType.Survival, amount * survivalXpPerConsumeAmount);
        }

        private void SetHunger(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, maxHunger);
            if (Mathf.Approximately(clamped, CurrentHunger))
            {
                return;
            }

            CurrentHunger = clamped;
            HungerChanged?.Invoke(CurrentHunger, maxHunger);
        }

        private void SetThirst(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, maxThirst);
            if (Mathf.Approximately(clamped, CurrentThirst))
            {
                return;
            }

            CurrentThirst = clamped;
            ThirstChanged?.Invoke(CurrentThirst, maxThirst);
        }
    }
}
