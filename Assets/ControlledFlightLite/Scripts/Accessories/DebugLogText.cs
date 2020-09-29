using UnityEngine;

namespace SparseDesign
{
    public class DebugLogText : MonoBehaviour
    {
        [SerializeField] bool m_disable = false;
        public void Print(string str)
        {
            if (!m_disable) Debug.Log(str);
        }

        public void Print(int x)
        {
            if (!m_disable) Debug.Log(x.ToString());
        }

        public void PrintWithTime(string str)
        {
            if (!m_disable) Debug.Log($"{str} ({UnityEngine.Time.time:F3})");
        }

        public void PrintWithTime(int x)
        {
            if (!m_disable) Debug.Log($"{x} ({UnityEngine.Time.time:F3})");
        }
    }
}