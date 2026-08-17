using System.Collections.Generic;
using UnityEngine;

namespace GameStart.Class
{
    // Placeholder class differentiation: tints the shared starter model per
    // class until real per-class art (Section 9 Asset Breakdown) exists.
    [RequireComponent(typeof(PlayerClassSelection))]
    public class ClassAppearance : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer targetRenderer;

        private static readonly Dictionary<PlayerClassType, Color> ClassTints = new Dictionary<PlayerClassType, Color>
        {
            { PlayerClassType.Warrior, new Color(0.65f, 0.16f, 0.10f) },
            { PlayerClassType.Ranger, new Color(0.16f, 0.5f, 0.22f) },
            { PlayerClassType.Mage, new Color(0.28f, 0.16f, 0.6f) },
        };

        private PlayerClassSelection classSelection;

        private void Awake()
        {
            classSelection = GetComponent<PlayerClassSelection>();
        }

        private void OnEnable()
        {
            classSelection.ClassSelected += ApplyTint;
        }

        private void OnDisable()
        {
            classSelection.ClassSelected -= ApplyTint;
        }

        private void ApplyTint(PlayerClassType classType)
        {
            if (targetRenderer == null || !ClassTints.TryGetValue(classType, out Color tint))
            {
                return;
            }

            foreach (Material material in targetRenderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", tint);
                }
            }
        }
    }
}
