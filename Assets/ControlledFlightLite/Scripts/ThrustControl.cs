using UnityEngine;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        abstract public class ThrustControl
        {
            /// <summary>
            /// Get a new instance of a sub-class of Thrustcontrol with the appropriate settings.
            /// </summary>
            /// <param name="missile">The missile to be controlled.</param>
            /// <param name="settings">Settings of the thrust control.</param>
            /// <returns></returns>
            public static ThrustControl GetNewInstance(GameObject missile, MotorSettings settings)
            {
                ThrustControl motor;
                switch (settings.m_motorType)
                {
                    case ThrustControl.MotorType.CONSTANTSPEED:
                        motor = new ConstantSpeed(missile, settings);
                        break;
                    default:
                        Debug.LogError($"Missing handling of motor type {settings.m_motorType}");
                        motor = new ConstantSpeed(missile, settings);
                        break;
                }
                return motor;
            }

            public enum MotorType
            {
                CONSTANTSPEED = 0,
            }

            [System.Serializable]
            public class MotorSettings// : ScriptableObject
            {
                public MotorType m_motorType = MotorType.CONSTANTSPEED;
                public bool m_limitFlightTime = false;
                public float m_flightTime = 10;

                public float m_speed = 1;
                public bool m_limitMotorAcceleration = false;
                public float m_maxAcceleration = 10;
            }

            static public float m_rho = 1.2250f;//https://en.wikipedia.org/wiki/Density_of_air

            public MotorSettings m_settings = default;

            public GameObject m_missile = default;
            public Rigidbody m_missileRb = default;

            public NormalEvent m_endFlightTimeEvent = new NormalEvent();

            private float m_tStart = float.MaxValue;
            private bool m_firstSample = true;
            private bool m_stageEnded = false;

            public ThrustControl(GameObject missile)
            {
                m_missile = missile;
                m_missileRb = m_missile.GetComponent<Rigidbody>();
                m_firstSample = true;
                m_stageEnded = false;
            }

            public ThrustControl(GameObject missile, MotorSettings settings) : this(missile)
            {

                m_settings = settings;
            }

            protected void CheckFlightTime()
            {
                if (m_firstSample)
                {
                    m_firstSample = false;
                    m_tStart = Time.time;
                }

                if (m_settings.m_limitFlightTime && !m_stageEnded && (Time.time > m_tStart + m_settings.m_flightTime))
                {
                    m_endFlightTimeEvent.Invoke();
                    m_stageEnded = true;
                }
            }

            abstract protected float GetCommand();

            virtual public void CommandMissile()
            {
                float command = GetCommand();
                var thrustDir = GetThrustDir();
                m_missileRb.AddForce(command * thrustDir, ForceMode.Acceleration);
                CheckFlightTime();//This should always be done in CommandMissile()!
            }

            virtual protected Vector3 GetThrustDir()
            {
                Vector3 velDir;
                var vel = m_missileRb.velocity;
                if (vel.sqrMagnitude > float.Epsilon)
                {
                    velDir = vel.normalized;
                }
                else
                {
                    velDir = m_missile.transform.rotation * Vector3.forward;//Maybe this is what should always be used?
                }
                return velDir;
            }
        }
    }
}