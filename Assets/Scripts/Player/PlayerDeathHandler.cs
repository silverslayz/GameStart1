using UnityEngine;
using GameStart.Flow;

namespace GameStart.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] private Vector3 respawnPoint = Vector3.zero;
        [SerializeField] private PermadeathUI permadeathUi;

        private PlayerHealth health;
        private PlayerNeeds needs;
        private CharacterController controller;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            needs = GetComponent<PlayerNeeds>();
            controller = GetComponent<CharacterController>();

            // Lives on a scene canvas, so a prefab instance starts with this null.
            permadeathUi = SceneLink.Resolve(permadeathUi);
        }

        private void OnEnable()
        {
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Died -= OnDied;
        }

        private void OnDied()
        {
            if (GameSessionSettings.HardModeEnabled)
            {
                if (permadeathUi != null)
                {
                    permadeathUi.Show("You Have Died\nHard Mode: no do-overs. This run has ended.");
                }

                return;
            }

            Respawn();
        }

        private void Respawn()
        {
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = respawnPoint;
                controller.enabled = true;
            }
            else
            {
                transform.position = respawnPoint;
            }

            health.Revive();

            if (needs != null)
            {
                needs.RestoreFull();
            }
        }
    }
}
