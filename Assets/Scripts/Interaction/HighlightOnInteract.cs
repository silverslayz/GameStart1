using UnityEngine;

namespace GameStart.Interaction
{
    [RequireComponent(typeof(Renderer))]
    public class HighlightOnInteract : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = Color.green;

        public void Highlight(GameObject interactor)
        {
            GetComponent<Renderer>().sharedMaterial.color = highlightColor;
        }
    }
}
