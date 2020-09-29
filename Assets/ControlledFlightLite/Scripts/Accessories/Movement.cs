using UnityEngine;

namespace SparseDesign
{
    public class Movement : MonoBehaviour
    {
        [Tooltip("Multiplicator")] [SerializeField] private float Gain = 1.0f;
        [Tooltip("Degrees/second")] [SerializeField] private Vector3 Rotation;
        [Tooltip("DistanceUnit/second")] [SerializeField] private Vector3 Velocity;
        private bool firstUpdate = true;

        private float lastUpdateTime;
        public Movement(Vector3 Velocity, Vector3 Rotation)
        {
            this.Rotation = Gain * Rotation;
            this.Velocity = Gain * Velocity;
        }

        public void FixedUpdate()
        {
            //transform.Rotate(Gain * Rotation * UnityEngine.Time.fixedDeltaTime);
            //transform.Translate(Gain * Velocity * UnityEngine.Time.fixedDeltaTime);
            UpdateMovement();
        }

        public void Update()
        {
            //transform.Rotate(Gain * Rotation * UnityEngine.Time.fixedDeltaTime);
            //transform.Translate(Gain * Velocity * UnityEngine.Time.fixedDeltaTime);
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (!firstUpdate)
            {
                firstUpdate = false;
                if (Time.time > lastUpdateTime)
                {
                    float dt = Time.time - lastUpdateTime;
                    transform.Rotate(Gain * Rotation * dt);
                    transform.Translate(Gain * Velocity * dt);
                }
            }

            lastUpdateTime = Time.time;
        }
    }
}