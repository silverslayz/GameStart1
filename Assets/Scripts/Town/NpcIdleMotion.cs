using UnityEngine;

namespace GameStart.Town
{
    public class NpcIdleMotion : MonoBehaviour
    {
        [SerializeField] private float bobHeight = 0.08f;
        [SerializeField] private float bobSpeed = 1.2f;
        [SerializeField] private float rotateSpeed = 20f;

        private Vector3 startLocalPosition;

        private void Awake()
        {
            startLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = startLocalPosition + new Vector3(0f, bobOffset, 0f);
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
