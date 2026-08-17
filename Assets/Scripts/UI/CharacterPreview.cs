using UnityEngine;

namespace GameStart.UI
{
    /// <summary>
    /// Renders a copy of the player model into a RenderTexture for the inventory's
    /// character panel.
    ///
    /// The copy lives far below the world rather than on a dedicated layer: naming a layer
    /// is a project-settings change, and this scene has none free by name. A tight far-clip
    /// plane on the preview camera keeps anything else out of frame, and the distance keeps
    /// the rig outside the main camera's range.
    /// </summary>
    public class CharacterPreview : MonoBehaviour
    {
        private const float RigDepth = -5000f;
        private const float CameraFov = 28f;

        private RenderTexture texture;
        private GameObject rig;
        private Transform modelRoot;

        public RenderTexture Texture => texture;

        /// <summary>Builds the rig. Returns false if there is no player model to copy.</summary>
        public bool Build(Transform source, int width, int height)
        {
            if (source == null)
            {
                return false;
            }

            texture = new RenderTexture(Mathf.Max(width, 16), Mathf.Max(height, 16), 24, RenderTextureFormat.ARGB32)
            {
                name = "CharacterPreviewRT",
                antiAliasing = 2
            };
            texture.Create();

            rig = new GameObject("CharacterPreviewRig");
            rig.transform.position = new Vector3(0f, RigDepth, 0f);

            var clone = Instantiate(source.gameObject, rig.transform);
            clone.name = "PreviewModel";
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
            clone.SetActive(true);
            StripBehaviours(clone);
            modelRoot = clone.transform;

            Bounds bounds = CalculateBounds(clone);
            FrameCamera(bounds);
            AddFill(bounds);

            return true;
        }

        /// <summary>
        /// Drops any gameplay scripts that came along with the clone so the preview copy
        /// can't run logic, take input, or register itself with game systems.
        /// </summary>
        private static void StripBehaviours(GameObject clone)
        {
            foreach (var behaviour in clone.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Destroy(behaviour);
            }

            foreach (var collider in clone.GetComponentsInChildren<Collider>(true))
            {
                Destroy(collider);
            }
        }

        private static Bounds CalculateBounds(GameObject clone)
        {
            var renderers = clone.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(clone.transform.position, Vector3.one * 2f);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private void FrameCamera(Bounds bounds)
        {
            var camGo = new GameObject("PreviewCamera");
            camGo.transform.SetParent(rig.transform, false);

            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.055f, 0.075f, 0.106f, 0f);
            cam.fieldOfView = CameraFov;
            cam.nearClipPlane = 0.05f;
            cam.orthographic = false;
            cam.targetTexture = texture;
            // Keeps the surrounding world out of frame even if something drifts nearby.
            cam.farClipPlane = Mathf.Max(bounds.size.magnitude * 4f, 12f);

            // Pull back far enough that the model's height fits the vertical FOV.
            float halfHeight = Mathf.Max(bounds.extents.y, 0.1f);
            float distance = halfHeight / Mathf.Tan(CameraFov * 0.5f * Mathf.Deg2Rad);
            distance *= 1.35f;

            Vector3 centre = bounds.center;
            camGo.transform.position = centre + new Vector3(0f, 0f, -distance);
            camGo.transform.LookAt(centre);
        }

        private void AddFill(Bounds bounds)
        {
            // Point lights rather than directional: a directional light would spill into the
            // main scene regardless of where this rig sits.
            CreateLight("KeyLight", bounds.center + new Vector3(-1.4f, 1.6f, -2.2f), new Color(0.85f, 0.92f, 1f), 2.4f, bounds);
            CreateLight("RimLight", bounds.center + new Vector3(1.8f, 1.1f, 1.6f), new Color(1f, 0.72f, 0.35f), 1.6f, bounds);
        }

        private void CreateLight(string name, Vector3 position, Color color, float intensity, Bounds bounds)
        {
            var go = new GameObject(name);
            go.transform.SetParent(rig.transform, false);
            go.transform.position = position;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = Mathf.Max(bounds.size.magnitude * 3f, 10f);
            light.shadows = LightShadows.None;
        }

        /// <summary>Spins the preview model, e.g. from a drag on the portrait.</summary>
        public void Rotate(float degrees)
        {
            if (modelRoot != null)
            {
                modelRoot.Rotate(0f, degrees, 0f, Space.Self);
            }
        }

        private void OnDestroy()
        {
            if (texture != null)
            {
                texture.Release();
                Destroy(texture);
            }

            if (rig != null)
            {
                Destroy(rig);
            }
        }
    }
}
