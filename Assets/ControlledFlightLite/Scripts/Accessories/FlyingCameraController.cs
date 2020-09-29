using UnityEngine;
using UnityEngine.Serialization;

namespace SparseDesign
{
    public class FlyingCameraController : MonoBehaviour
    {
        [SerializeField] public GameObject m_object = default;
        private Rigidbody m_rb = default;
        [FormerlySerializedAsAttribute("CameraDistanceMin")]
        [SerializeField] float CameraDistance = 1.5f;
        //[SerializeField] float CameraDistanceMax = 3.0f;

        [Tooltip("Angle above [degrees]")] [SerializeField] float m_cameraAngle = 25f;

        //private Vector3 Offset = default;
        private Filter m_filterCameraMovement = default;
        //private Filter m_filterCameraZoom = default;

        [SerializeField] CameraType m_cameraType = default;

        /// <summary>
        /// Type of camera
        /// </summary>
        enum CameraType
        {
            FIRSTPERSON,
            THIRDPERSON
        }

        void Start()
        {
            if (m_object == null) return;
            m_rb = m_object.GetComponent<Rigidbody>();

            m_filterCameraMovement = new Filter(4.0f, IsAngles: true);
            //m_filterCameraZoom = new Filter(1.0f, IsAngles: false);
        }

        void LateUpdate()
        {
            if (m_object == null) return;
            Vector3 FilteredAngle;
            if (m_cameraType == CameraType.FIRSTPERSON)
            {
                //This doesnt seem right
                //Quaternion DesiredRotation = Quaternion.Euler(-m_object.transform.eulerAngles.z, m_object.transform.eulerAngles.y + 90f, m_object.transform.eulerAngles.x);

                transform.position = m_object.transform.position;
                transform.rotation = m_object.transform.rotation;
            }
            else
            {
                if (m_rb == null) return;

                float ScaleVelToDistance = 1.0f / 1.0f;

                float distance = Mathf.Sqrt(m_rb.velocity.x * m_rb.velocity.x + m_rb.velocity.z * m_rb.velocity.z) * ScaleVelToDistance;

                distance = CameraDistance;// Mathf.Clamp(distance, CameraDistanceMin, CameraDistanceMax);
                //distance = m_filterCameraZoom.UpdateFilter(distance);

                float desiredAngle = MathHelp.ConvertDegreesToRadians(m_object.transform.eulerAngles.y);
                FilteredAngle = new Vector3(0.0f, MathHelp.ConvertRadiansToDegrees(m_filterCameraMovement.UpdateFilter(desiredAngle)), 0.0f);
                Quaternion rotation = Quaternion.Euler(FilteredAngle);

                Vector3 offset;

                offset.x = 0;
                offset.y = Mathf.Sin(m_cameraAngle * Mathf.Deg2Rad);
                offset.z = -Mathf.Cos(m_cameraAngle * Mathf.Deg2Rad);

                transform.position = m_object.transform.position + (rotation * offset * distance);

                transform.LookAt(m_object.transform.position);
            }
        }
    }
}
