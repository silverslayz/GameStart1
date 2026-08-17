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

        /// <summary>Drops a stack at a position, nudged slightly so it doesn't sit inside the player.</summary>
        public static DroppedItem Spawn(GearItem item, int count, Vector3 position)
        {
            if (count <= 0)
            {
                return null;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Dropped_{item.Name}";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.35f;
            go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.85f, 0.66f, 0.25f);
            }

            var dropped = go.AddComponent<DroppedItem>();
            dropped.item = item;
            dropped.count = count;
            return dropped;
        }
    }
}
