using UnityEngine;

namespace SparseDesign
{
    public class CreateObject : MonoBehaviour
    {
        [SerializeField] public GameObject m_prefab = default;
        //[SerializeField] public GameObject m_lastCreated = default;

        public void Create(Vector3 pos, Quaternion rot)
        {
            if (!m_prefab)
            {
                //m_lastCreated = null;
                return;
            }
            var obj = Instantiate(original: m_prefab, position: pos, rotation: rot);
            obj.SetActive(true);
        }

        public void Create(Vector3 pos) => Create(pos, Quaternion.identity);
        public void Create(GameObject objPos) => Create(objPos.transform.position, objPos.transform.rotation);
    }
}