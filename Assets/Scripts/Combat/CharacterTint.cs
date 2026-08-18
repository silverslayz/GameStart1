using UnityEngine;

namespace GameStart.Combat
{
    /// <summary>
    /// Colours a character so hostility is readable at a glance: friendly blue, hostile red.
    ///
    /// Uses a MaterialPropertyBlock rather than touching renderer.materials. Reading
    /// .materials instantiates a copy per renderer, which leaks a material for every
    /// monster in a dungeon; a property block changes the colour with no allocation and
    /// leaves the shared material untouched.
    /// </summary>
    public class CharacterTint : MonoBehaviour
    {
        public static readonly Color Friendly = new Color(0.22f, 0.45f, 0.85f);
        public static readonly Color Hostile = new Color(0.72f, 0.16f, 0.14f);

        [SerializeField] private Color tint = Friendly;

        [Tooltip("Renderers to leave alone - eyes, emissive parts, anything whose colour carries meaning.")]
        [SerializeField] private Renderer[] excluded;

        private MaterialPropertyBlock block;

        private void Start()
        {
            Apply(tint);
        }

        public void Apply(Color colour)
        {
            tint = colour;
            block ??= new MaterialPropertyBlock();

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (IsExcluded(renderer))
                {
                    continue;
                }

                renderer.GetPropertyBlock(block);
                // URP Lit uses _BaseColor; _Color covers anything still on a built-in shader.
                block.SetColor("_BaseColor", colour);
                block.SetColor("_Color", colour);
                renderer.SetPropertyBlock(block);
            }
        }

        private bool IsExcluded(Renderer renderer)
        {
            if (excluded == null)
            {
                return false;
            }

            foreach (var e in excluded)
            {
                if (e == renderer)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
