using UnityEngine;

namespace SparseDesign
{
    [RequireComponent(typeof(Rigidbody))]

    public class ChangeAcceleration : MonoBehaviour
    {
        private Vector3 m_lastVelocity = default;
        private bool m_firstSample = true;

        [SerializeField] private float m_factorChange = 0f;

        private Rigidbody m_Rb = null;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            var vel = m_Rb.velocity;
            if (m_firstSample)
            {
                m_firstSample = false;
                m_lastVelocity = vel;
            }

            var acc = (vel - m_lastVelocity) / Time.fixedDeltaTime;

            Vector3 m_accChange = m_factorChange * new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

            m_Rb.AddForce(Vector3.Scale(acc, m_accChange), ForceMode.Acceleration);

            m_lastVelocity = vel;
        }
    }
}