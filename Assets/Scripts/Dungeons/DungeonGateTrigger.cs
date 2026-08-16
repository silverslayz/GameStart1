using UnityEngine;
using UnityEngine.Events;
using GameStart.Skills;

namespace GameStart.Dungeons
{
    [RequireComponent(typeof(Collider))]
    public class DungeonGateTrigger : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> onEntryGranted;
        [SerializeField] private UnityEvent<string> onEntryDenied;

        private void OnTriggerEnter(Collider other)
        {
            var skills = other.GetComponent<PlayerSkills>();
            var progress = other.GetComponent<PlayerDungeonProgress>();
            if (skills == null || progress == null)
            {
                return;
            }

            DungeonDefinition dungeon = progress.CurrentDungeon;

            if (progress.TryEnterCurrentDungeon(skills))
            {
                onEntryGranted?.Invoke(dungeon.Name);
            }
            else
            {
                onEntryDenied?.Invoke(dungeon.Name);
            }
        }
    }
}
