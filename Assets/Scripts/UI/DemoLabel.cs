using UnityEngine;

namespace GameStart.UI
{
    /// <summary>
    /// A name plate that hovers over one exhibit in the demo room and turns to face the
    /// camera. Deliberately a 3D TextMesh rather than the world-space canvas the HUD uses:
    /// there can be twenty of these on screen at once, they never need input, and they
    /// should be occluded by walls like any other object in the room.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class DemoLabel : MonoBehaviour
    {
        [SerializeField] private bool faceCamera = true;

        private TextMesh text;

        public void SetText(string value)
        {
            Text.text = value;
        }

        private TextMesh Text => text != null ? text : (text = GetComponent<TextMesh>());

        private void LateUpdate()
        {
            if (!faceCamera)
            {
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            // Rotate about Y only. Tilting to match a camera looking down would make the
            // plate lie back and read as skewed from across the room.
            Vector3 toCamera = camera.transform.position - transform.position;
            toCamera.y = 0f;
            if (toCamera.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(-toCamera.normalized, Vector3.up);
        }
    }
}
