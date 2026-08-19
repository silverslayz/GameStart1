using UnityEngine;
using GameStart.Combat;

namespace GameStart.UI
{
    /// <summary>
    /// Damage numbers. Rides on <see cref="FloatingWorldText"/> for its canvas and
    /// world-to-screen projection, so any script can call this without a serialized
    /// reference and nothing has to be placed in the shared scene.
    /// </summary>
    public static class FloatingCombatText
    {
        public static readonly Color DamageColor = new Color(1f, 0.96f, 0.88f, 1f);

        /// <summary>
        /// The same gold BestiaryUI uses for "Weakness discovered", so the number and the
        /// bestiary entry that explains it read as the same idea.
        /// </summary>
        public static readonly Color WeaknessDamageColor = new Color(1f, 0.85f, 0.3f, 1f);

        /// <summary>
        /// Weakness hits are only 25% bigger in value, which is easy to miss between two
        /// numbers a moment apart - so they're drawn larger as well as recolored, and stay
        /// distinguishable for players who can't rely on the color difference.
        /// </summary>
        private const float WeaknessSizeScale = 1.3f;

        /// <summary>Spawns a damage number above the target's visible bounds.</summary>
        public static void ShowDamage(Transform target, float amount, DamageFlavor flavor = DamageFlavor.Normal)
        {
            if (target == null)
            {
                return;
            }

            ShowDamage(FloatingWorldText.AnchorAbove(target, 0.25f, out _), amount, flavor);
        }

        /// <summary>Spawns a damage number rising from a world position.</summary>
        public static void ShowDamage(Vector3 worldPosition, float amount, DamageFlavor flavor = DamageFlavor.Normal)
        {
            // Damage is tracked as a float but reads as a whole number, and any landed hit
            // should show at least 1 rather than a "0" that looks like the swing whiffed.
            int rounded = Mathf.Max(1, Mathf.RoundToInt(amount));
            bool weakness = flavor == DamageFlavor.Weakness;

            Show(worldPosition,
                rounded.ToString(),
                weakness ? WeaknessDamageColor : DamageColor,
                weakness ? WeaknessSizeScale : 1f);
        }

        public static void Show(Vector3 worldPosition, string content, Color color, float sizeScale = 1f)
        {
            FloatingWorldText.Runner.SpawnRising(worldPosition, content, color, sizeScale);
        }
    }
}
