using UnityEngine;

namespace GameStart.Flow
{
    /// <summary>
    /// Resolves references a prefab cannot serialize.
    ///
    /// A prefab asset can only store references to project assets, never to objects living in
    /// a scene. When the player was converted to a prefab, its fields pointing at scene
    /// objects - the permadeath canvas, the class-selection canvas, the camera overlay quads -
    /// survived only as overrides on that one instance. A freshly instantiated prefab, which
    /// is what multiplayer spawning and any second scene will do, would come up with those
    /// fields null.
    ///
    /// Every call keeps an existing wired reference and only falls back to a lookup when the
    /// field is empty, so scene-authored wiring stays authoritative and avoids the search.
    /// </summary>
    public static class SceneLink
    {
        /// <summary>Returns the field if already wired, otherwise the single instance in the loaded scenes.</summary>
        public static T Resolve<T>(T current) where T : Object
        {
            if (current != null)
            {
                return current;
            }

            return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        }

        /// <summary>
        /// Finds a named overlay quad parented to the main camera. These can't be resolved by
        /// type - they're plain Renderers, and the camera holds more than one.
        /// </summary>
        public static Renderer ResolveCameraOverlay(Renderer current, string childName)
        {
            if (current != null)
            {
                return current;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                return null;
            }

            Transform child = cam.transform.Find(childName);
            return child != null ? child.GetComponent<Renderer>() : null;
        }
    }
}
