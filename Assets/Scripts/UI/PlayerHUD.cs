using UnityEngine;
using GameStart.Player;

namespace GameStart.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerStamina stamina;
        [SerializeField] private PlayerNeeds needs;

        [SerializeField] private StatusBar healthBar;
        [SerializeField] private StatusBar staminaBar;
        [SerializeField] private StatusBar hungerBar;
        [SerializeField] private StatusBar thirstBar;

        private void OnEnable()
        {
            if (health != null)
            {
                health.HealthChanged += healthBar.SetValue;
                healthBar.SetValue(health.CurrentHealth, health.MaxHealth);
            }

            if (stamina != null)
            {
                stamina.StaminaChanged += staminaBar.SetValue;
                staminaBar.SetValue(stamina.CurrentStamina, stamina.MaxStamina);
            }

            if (needs != null)
            {
                needs.HungerChanged += hungerBar.SetValue;
                needs.ThirstChanged += thirstBar.SetValue;
                hungerBar.SetValue(needs.CurrentHunger, needs.MaxHunger);
                thirstBar.SetValue(needs.CurrentThirst, needs.MaxThirst);
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= healthBar.SetValue;
            }

            if (stamina != null)
            {
                stamina.StaminaChanged -= staminaBar.SetValue;
            }

            if (needs != null)
            {
                needs.HungerChanged -= hungerBar.SetValue;
                needs.ThirstChanged -= thirstBar.SetValue;
            }
        }
    }
}
