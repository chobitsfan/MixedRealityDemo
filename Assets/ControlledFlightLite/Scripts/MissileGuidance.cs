using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        abstract public class MissileGuidance
        {
            /// <summary>
            /// Get a new instance of a sub-class of Thrustcontrol with the appropriate settings.
            /// </summary>
            /// <param name="missile">The missile to be controlled.</param>
            /// <param name="settings">Settings of the thrust control.</param>
            /// <returns></returns>
            public static MissileGuidance GetNewInstance(GameObject missile, GuidanceSettings settings)
            {
                MissileGuidance guidance;

                if (settings.m_targetType == MissileGuidance.TargetType.TARGET)
                {
                    switch (settings.m_guidanceType)
                    {
                        case GuidanceType.PROPORTIONALNAVIGATION:
                            guidance = new ProportionalNavigation(missile: missile, settings: settings);
                            break;
                        default:
                            Debug.LogError($"Missing handling of guidance type {settings.m_guidanceType}");
                            guidance = new ProportionalNavigation(missile: missile, settings: settings);
                            break;
                    }
                }
                else if (settings.m_targetType == TargetType.PATH)
                {
                    guidance = new PathFollow(missile: missile, settings: settings);
                }
                else
                {
                    Debug.LogError($"Missing handling of guidance type {settings.m_guidanceType}");
                    guidance = new ProportionalNavigation(missile, settings);
                }

                return guidance;
            }

            public enum GuidanceType
            {
                PROPORTIONALNAVIGATION = 0,
            }

            public enum TargetType
            {
                TARGET = 0,
                PATH = 1
            }

            [System.Serializable]
            /// <summary>
            /// All settings for MissileGuidance class
            /// </summary>
            public class GuidanceSettings
            {
                //General settings, always relevant
                public TargetType m_targetType = TargetType.TARGET;

                public bool m_limitAcceleration = true;
                public float m_maxAcceleration = 100;

                //Settings for target guidance
                public GuidanceType m_guidanceType = default;
                public GameObject m_target = default;

                //Settings for path following
                public bool m_loopPath = false;
                [Range(0.01f, 10)] public float m_tTurnIn = 0.5f;
                [Range(-1, 1)] public float m_turnMarginSimple = 0f;
                [Range(2, 20)] public float m_pathN = 4f;//Navigation Constant for path following

                //Settings for CLOS
                [Range(2, 20)] public float m_CLOSN = 4f;//Navigation Constant for CLOS

                public List<GameObject> m_pathObjs = new List<GameObject>();

                //Settings for specific algorithms
                [Range(2, 20)] public float m_N = 4;//Navigation Constant

                /// <summary>
                /// Position of path in idx. Do not check idx validity.
                /// </summary>
                /// <param name="idx"></param>
                /// <returns></returns>
                public Vector3 GetPathPos(int idx)
                {
                    return m_pathObjs[idx].transform.position;
                }

                /// <summary>
                /// Position of path in idx. Loops the index, i.e. when value over path count-1 starts from 0 again.
                /// </summary>
                /// <param name="idx"></param>
                /// <returns></returns>
                public Vector3 GetPathPosLoop(int idx)
                {
                    return GetPathPos(idx % this.PathCount());
                }

                /// <summary>
                /// The number of path positions.
                /// </summary>
                public int PathCount()
                {
                    return m_pathObjs.Count;
                }

                private List<Vector3> m_lastObjPos = new List<Vector3>();

                /// <summary>
                /// Get the positions of the path
                /// </summary>
                /// <returns></returns>
                public List<Vector3> GetPathPositions()//Todoi: optimize
                {
                    m_lastObjPos.Clear();
                    foreach (var o in m_pathObjs)
                    {
                        if (o) m_lastObjPos.Add(o.transform.position);
                    }
                    return m_lastObjPos;
                }
            }

            public GameObject m_missile = default;
            protected Rigidbody m_missileRb = default;

            protected KinematicEstimator m_targetState;

            protected Vector3 m_lastCommandUsedForAttitude = default;

            /// <summary>
            /// The acceleration command sent to Unity physics engine, i.e. the current guidance command used.
            /// Can be used by external functionality.
            /// </summary>
            public Vector3 m_currentCommand { private set; get; } = Vector3.zero;

            [SerializeField] public GuidanceSettings m_settings = new GuidanceSettings();

            /// <summary>
            /// Variables used in CLOS
            /// </summary>
            public Vector3 m_startPos { protected set; get; } = default;
            public Vector3 m_startDir { protected set; get; } = default;
            public Vector3 m_aimPoint { protected set; get; } = default;

            /// <summary>
            /// //Invoked when a new waypoint is used. The value is the first waypoint on the current leg. 
            /// </summary>
            public IntEvent m_eventNextWayPoint = new IntEvent();

            private bool m_firstCommand = true;
            private float m_lastTime = default;

            public List<Vector3> m_debugPos { protected set; get; } = new List<Vector3>();
            public List<Vector3> m_debugLine { protected set; get; } = new List<Vector3>();

            public MissileGuidance(GameObject missile, GuidanceSettings settings)
            {
                if (!missile) Debug.LogError("A valid missile object must be provided when instantiating missile guidance");
                m_missile = missile;

                if (settings == null) Debug.LogError("Valid settings must be provided whn instantiating missile guidance");

                m_settings = settings;

                m_missileRb = m_missile.GetComponent<Rigidbody>();
                if (m_settings.m_target) m_targetState = new KinematicEstimator(m_settings.m_target);
            }

            /// <summary>
            /// Align object z-axis with velocity.
            /// Also handle Bank-To-Turn.
            /// </summary>
            public void AttitudeAdjustment()
            {
                float dt = (m_firstCommand) ? 0f : Time.time - m_lastTime;

                if (m_missileRb.velocity.sqrMagnitude < float.Epsilon) return;
                Vector3 up;
                up = Vector3.up;

                var oldRot = m_missile.transform.rotation;

                Quaternion newRot;
                if (m_firstCommand)
                    newRot = oldRot;
                else
                    newRot = Quaternion.Lerp(oldRot, Quaternion.LookRotation(m_missileRb.velocity, up), dt / 0.1f);//Limit rotation to avoid odd behaviour around vertical (gimbal lock).

                m_missileRb.MoveRotation(newRot);
            }

            /// <summary>
            /// This is the main method to be periodically called to guide the missile.
            /// CommandMissile should be called from FixedUpdate to guide the object. It is not necessary to be called from FixedUpdate, butn care must be taken if not.
            /// </summary>
            /// <param name="doControl">If true, the missile is controlled, otherwise the guidance system is off</param>
            public void CommandMissile(bool doControl)//May be set to virtual if some algorithm needs it 
            {
                var command = GetCommand();

                AttitudeAdjustment();
                if (!doControl) return;

                command = VectorCalculation.LimitMagnitude(command, 1000f);//Limit to 1000 (~100g) to avoid accelerations the physics engine can't handle without strange effects
                m_missileRb.AddForce(command, ForceMode.Acceleration);

                m_currentCommand = command;

                m_firstCommand = false;
                m_lastTime = Time.time;
            }

            /// <summary>
            /// This is where the main guidance algorithms are calculated
            /// </summary>
            /// <returns>Acceleration command [m/s2]</returns>
            abstract protected Vector3 GetCommand();

            /// <summary>
            /// Is the missile currently going for the target? Usually yes, but for path only the last waypoint counts as the target.
            /// </summary>
            /// <returns></returns>
            virtual public bool AimingForTarget()
            {
                return true;
            }

            /// <summary>
            /// Provides the state (position and velocity) of the current target.
            /// The current target can be a target or the current waypoint.
            /// </summary>
            /// <param name="pos"></param>
            /// <param name="vel"></param>
            virtual public void GetTargetState(out Vector3 pos, out Vector3 vel)
            {
                if (m_targetState != null)
                {
                    pos = m_targetState.GetPos();
                    vel = m_targetState.GetVel();
                }
                else
                {
                    pos = Vector3.zero;
                    vel = Vector3.zero;
                }
            }
        }
    }
}