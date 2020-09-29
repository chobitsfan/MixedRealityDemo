using UnityEngine;

namespace SparseDesign
{
    public class KinematicEstimator //: MonoBehaviour
    {
        public GameObject m_obj = default;
        Rigidbody m_rb = default;

        float m_lastTime = default;
        Vector3 m_lastPos = default;
        Vector3 m_lastVel = default;
        Vector3 m_lastAcc = default;

        bool m_firstSample = true;

        public KinematicEstimator(GameObject obj)
        {
            if (!obj) Debug.LogError("A valid object must be provided when instantiating kinematic estimator.");

            m_obj = obj;
            m_lastTime = UnityEngine.Time.time;
            m_lastPos = m_obj.transform.position;
            m_rb = m_obj.GetComponent<Rigidbody>();

            m_lastVel = m_rb ? m_rb.velocity : Vector3.zero;
            m_lastAcc = Vector3.zero;
            m_firstSample = true;
        }

        void UpdateState()
        {
            float t = UnityEngine.Time.time;
            float dt = t - m_lastTime;
            if (dt <= float.Epsilon) return;
            var newPos = m_obj.transform.position;
            var newVel = m_rb ? m_rb.velocity : (newPos - m_lastPos) / dt;
            m_lastAcc = m_firstSample ? Vector3.zero : (newVel - m_lastVel) / dt;

            m_lastPos = newPos;
            m_lastVel = newVel;
            m_lastTime = t;
            m_firstSample = false;
        }

        public Vector3 GetPos()
        {
            float dt = UnityEngine.Time.time - m_lastTime;
            if (dt > float.Epsilon)
            {
                UpdateState();
            }
            return m_lastPos;
        }

        public Vector3 GetVel()
        {
            float dt = UnityEngine.Time.time - m_lastTime;
            if (dt > float.Epsilon)
            {
                UpdateState();
            }
            return m_lastVel;
        }

        public Vector3 GetAcc()
        {
            float dt = UnityEngine.Time.time - m_lastTime;
            if (dt > float.Epsilon)
            {
                UpdateState();
            }
            return m_lastAcc;
        }
    }
}