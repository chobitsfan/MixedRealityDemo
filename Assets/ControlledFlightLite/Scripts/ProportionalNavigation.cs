using UnityEngine;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        /// <summary>
        ///PN algorithm (https://en.wikipedia.org/wiki/Proportional_navigation)
        ///Using energy conserving control, i.e. command orthogonal to velocity.
        ///The energy can not perfectly be conserved due to Unitys physics simulation
        /// </summary>
        public class ProportionalNavigation : MissileGuidance
        {
            public ProportionalNavigation(GameObject missile, GuidanceSettings settings) : base(missile, settings)
            {
                if (!settings.m_target) Debug.LogError($"A valid target object must be provided when instantiating PN. (missile obj: {(missile ? missile.name : null)})");
            }

            protected override Vector3 GetCommand()
            {
                if (m_targetState == null) m_targetState = new KinematicEstimator(m_settings.m_target);

                Vector3 missilePos = m_missile.transform.position;
                Vector3 R = m_targetState.GetPos() - missilePos;
                Vector3 Vm = m_missileRb.velocity;
                float speedM = Vm.magnitude;

                Vector3 a;
                if ((R.sqrMagnitude > float.Epsilon) && (speedM > float.Epsilon))
                {
                    Vector3 Vt = m_targetState.GetVel();//Target vel
                    Vector3 Vr = Vt - Vm;//Relative vel between target and missile
                    var VrMagnitude = Vr.magnitude;
                    Vector3 O = Vector3.Cross(R, Vr) / R.sqrMagnitude;

                    a = -m_settings.m_N * VrMagnitude * Vector3.Cross(Vm.normalized, O);

                    if (m_settings.m_limitAcceleration) a = VectorCalculation.LimitMagnitude(a, m_settings.m_maxAcceleration);
                }
                else
                {
                    a = Vector3.zero;
                }

                return a;
            }
        }
    }
}