using UnityEngine;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        public class ConstantSpeed : ThrustControl
        {
            private float m_lastTime = 0f;
            private int m_sampleNo = 0;
            private float m_lastSpeed = default;

            public ConstantSpeed(GameObject missile, MotorSettings settings) : base(missile, settings) { }

            protected override float GetCommand()
            {
                float newSpeed;
                float currentSpeed = m_missileRb.velocity.magnitude;

                if (m_settings.m_limitMotorAcceleration)
                {
                    //Wait a few sample for launch speed in MissileSupervisor have an effect
                    if (m_sampleNo < 2)
                    {
                        newSpeed = currentSpeed;
                        m_sampleNo++;
                    }
                    else
                    {
                        float maxChange = m_settings.m_maxAcceleration * (UnityEngine.Time.time - m_lastTime);

                        newSpeed = Mathf.Clamp(m_settings.m_speed, m_lastSpeed - maxChange, m_lastSpeed + maxChange);
                    }
                }
                else
                {
                    newSpeed = m_settings.m_speed;
                }

                m_lastTime = UnityEngine.Time.time;
                m_lastSpeed = newSpeed;

                return (newSpeed - currentSpeed);
            }

            override public void CommandMissile()
            {
                float command = GetCommand();
                var thrustDir = GetThrustDir();
                m_missileRb.AddForce(command * thrustDir, ForceMode.VelocityChange);
                CheckFlightTime();
            }
        }
    }
}