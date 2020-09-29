using UnityEngine;

namespace SparseDesign
{
    public class ShowObject : MonoBehaviour
    {
        [SerializeField] private float size = 0.2f;
        [SerializeField] private Color color = Color.black;

        private void OnDrawGizmos()
        {
            if (size > float.Epsilon)
            {
                Gizmos.color = color;
                Gizmos.DrawSphere(transform.position, size);
            }
        }
    }
}