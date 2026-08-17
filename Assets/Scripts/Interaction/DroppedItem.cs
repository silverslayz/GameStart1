using UnityEngine;
using GameStart.Class;

namespace GameStart.Interaction
{
    /// <summary>
    /// An item the player has dropped, sitting in the world until picked back up.
    ///
    /// Exists so discarding something from the inventory is reversible. Dropping used to
    /// clear the slot outright, which destroyed the whole stack with no way to recover it.
    /// </summary>
    public class DroppedItem : MonoBehaviour, IInteractable
    {
        private GearItem item;
        private int count;

        public string InteractionPrompt => count > 1
            ? $"Pick up {item.Name} x{count}"
            : $"Pick up {item.Name}";

        public void Interact(GameObject interactor)
        {
            var inventory = interactor.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                return;
            }

            // Take back as much as fits. If the bag fills up partway, the rest stays on the
            // ground rather than evaporating.
            while (count > 0 && inventory.AddItem(item))
            {
                count--;
            }

            if (count <= 0)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Loaded from Resources because Spawn is static - there is no scene object to hold a reference.</summary>
        private const string PrefabResourcePath = "Prefabs/DroppedItem";

        private static GameObject prefabCache;

        private static GameObject LoadPrefab()
        {
            if (prefabCache == null)
            {
                prefabCache = Resources.Load<GameObject>(PrefabResourcePath);
                if (prefabCache == null)
                {
                    Debug.LogError($"DroppedItem prefab missing at Resources/{PrefabResourcePath} - items cannot be dropped.");
                }
            }

            return prefabCache;
        }

        /// <summary>
        /// Drops a stack at a position. Returns null if nothing could be spawned, which callers
        /// must treat as a failure and hand the items back - they have usually already been
        /// removed from the inventory by the time this runs.
        /// </summary>
        public static DroppedItem Spawn(GearItem item, int count, Vector3 position)
        {
            if (count <= 0)
            {
                return null;
            }

            GameObject prefab = LoadPrefab();
            if (prefab == null)
            {
                return null;
            }

            var go = Instantiate(prefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            go.name = $"Dropped_{item.Name}";

            var dropped = go.GetComponent<DroppedItem>();
            if (dropped == null)
            {
                Debug.LogError("DroppedItem prefab has no DroppedItem component.");
                Destroy(go);
                return null;
            }

            dropped.item = item;
            dropped.count = count;
            return dropped;
        }
    }
}
