using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        [RequireComponent(typeof(Rigidbody))]
        [AddComponentMenu("Controlled Flight/MissileSupervisor")]

        /// <summary>
        /// Main component for Controlled Flight (https://sparsedesign.com/controlled-flight/)
        /// </summary>
        public class MissileSupervisor : MonoBehaviour
        {
            /// <summary>
            /// Enum for guidance phase
            /// </summary>
            public enum PhaseEnum
            {
                PRETRACK,
                TRACKING,
                FLIGHT,
                DONE,
            }

            /// <summary>
            /// Enum for state of intercept
            /// </summary>
            enum InterceptEnum
            {
                NONE,
                INRANGE,
                OUTSIDERANGE
            }

            /// <summary>
            /// Enum for launch type
            /// </summary>
            public enum LaunchType
            {
                TARGETDIRECTION = 0,
                CUSTOMDIRECTION = 2,
                NONE = 4
            }

            [SerializeField] public bool m_debug = false;

            [SerializeField] public bool m_autoLaunch = true;
            [SerializeField] public float m_trackingDelay = 0f;
            [SerializeField] public bool m_stopAtIntercept = false;
            [SerializeField] public bool m_destroyAtHit = false;
            [SerializeField] [Tooltip("[meter]")] [Range(0f, 1000f)] public float m_interceptRange = 1;

            [SerializeField] public bool m_limitFlightTime = false;
            [SerializeField] public float m_flightTime = 100;//Todo: when this is changed, set limiktation in m_guidance

            [SerializeField] public MissileGuidance.GuidanceSettings m_guidanceSettings = new MissileGuidance.GuidanceSettings();

            public int m_currentMotorStage { private set; get; } = 0;
            [SerializeField] public List<ThrustControl.MotorSettings> m_motorStages = new List<ThrustControl.MotorSettings>();

            [SerializeField] public LaunchType m_launchType = default;
            [SerializeField] public Vector3 m_launchCustomDir = default;
            [SerializeField] public GameObject m_launchObject = default;

            [SerializeField] public float m_launchSpeed = 0f;

            private MissileGuidance m_guidance = default;
            private ThrustControl m_motor = default;

            private KinematicEstimator m_targetState = null;
            private InterceptEnum m_interceptStatus = InterceptEnum.NONE;

            public PhaseEnum m_phase { private set; get; } = PhaseEnum.PRETRACK;

            public Rigidbody m_rb { private set; get; } = default;

            private float m_launchTime = default;
            private bool m_hasMoved = false;

            ///<summary> An event that is invoked when then missile hits the target. The object return is the missile.</summary>
            public GameObjectEvent m_interceptEvent = new GameObjectEvent();
            ///<summary> An event that is invoked when then missile is launched.</summary>
            public NormalEvent m_launchEvent = new NormalEvent();

            private void Awake()
            {
                m_rb = GetComponent<Rigidbody>();
                m_rb.angularDrag = 0;//Do not use the angular drag simulation in Unity, it wouldn't make any sense in this context.
                m_rb.useGravity = false;//All guidance would fully compensate for this (maneouver limitations do not account for gravity which is a simplification that has been made)
                m_rb.isKinematic = false;//Obviously...
                if (m_guidanceSettings.m_target) m_targetState = new KinematicEstimator(m_guidanceSettings.m_target);
            }

            void Start()
            {
                if (m_autoLaunch) StartLaunchSequence();
            }

            void Track()
            {
                switch (m_launchType)
                {
                    case LaunchType.TARGETDIRECTION:
                        if (m_guidanceSettings.m_targetType == MissileGuidance.TargetType.TARGET && (m_targetState != null))
                        {
                            MissileSupervisor.Track(m_rb, m_launchType, m_targetState.GetPos(), null);
                        }
                        else if ((m_guidanceSettings.m_targetType == MissileGuidance.TargetType.PATH) && (m_guidanceSettings.PathCount() > 0))
                        {
                            MissileSupervisor.Track(m_rb, m_launchType, m_guidanceSettings.GetPathPos(0), null);
                        }
                        break;
                    case LaunchType.CUSTOMDIRECTION:
                        MissileSupervisor.Track(m_rb, m_launchType, m_launchCustomDir, null);
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// Rotates according to LaunchType. No rotation angle limit.
            /// </summary>
            /// <param name="rb">Rigidbody that is to be rotated</param>
            /// <param name="launchType"></param>
            /// <param name="vector">Used for LaunchType.TARGETDIRECTION, LaunchType.CUSTOMDIRECTION</param>
            /// <param name="obj">Used for LaunchType.OBJECT</param>
            public static void Track(Rigidbody rb, LaunchType launchType, Vector3 vector, GameObject obj) => Track(rb, launchType, vector, obj, false, 0);

            /// <summary>
            /// Rotates according to LaunchType
            /// </summary>
            /// <param name="rb">Rigidbody that is to be rotated</param>
            /// <param name="launchType"></param>
            /// <param name="vector">Used for LaunchType.TARGETDIRECTION, LaunchType.CUSTOMDIRECTION</param>
            /// <param name="obj">Used for LaunchType.OBJECT</param>
            /// <param name="limitRotationAngle">Limit rotation angle/param>
            /// <param name="maxAngle">Maximum rotation angle [rad]/param>
            public static void Track(Rigidbody rb, LaunchType launchType, Vector3 vector, GameObject _, bool __, float ____)
            {
                Quaternion rot;
                bool rotate = true;
                switch (launchType)
                {
                    case LaunchType.TARGETDIRECTION:
                        rot = Quaternion.LookRotation(vector - rb.gameObject.transform.position);
                        break;
                    case LaunchType.CUSTOMDIRECTION:
                        rot = Quaternion.LookRotation(vector);
                        break;
                    default:
                        rot = Quaternion.identity;
                        rotate = false;
                        break;
                }

                if (rotate) rb.MoveRotation(rot);
            }

            private void FixedUpdate()
            {
                if (m_phase == PhaseEnum.TRACKING)
                {
                    Track();
                }

                if (!m_hasMoved && m_phase == PhaseEnum.FLIGHT && m_rb.velocity.sqrMagnitude > 1e-6)
                {
                    m_hasMoved = true;
                }

                if (m_motor != null)
                {
                    m_motor.CommandMissile();
                }

                if (m_hasMoved && (m_phase == PhaseEnum.FLIGHT))
                {
                    m_guidance.CommandMissile(doControl: true);

                    CheckIntercept();
                    if (m_stopAtIntercept && m_interceptStatus != InterceptEnum.NONE) m_phase = PhaseEnum.DONE;
                    if (m_destroyAtHit && (m_interceptStatus == InterceptEnum.INRANGE)) Destroy(this.gameObject);

                    if (m_limitFlightTime && (Time.time > m_launchTime + m_flightTime))
                    {
                        m_phase = PhaseEnum.DONE;
                    }
                }
            }

            IEnumerator LaunchSequence()
            {
                TrackTarget();
                yield return new WaitForSeconds(Mathf.Max(Time.fixedDeltaTime + float.Epsilon, m_trackingDelay));//At least one physics update before tracking
                Launch();
            }

            /// <summary>
            /// Starts the launch sequence, if possible.
            /// Tracks for the required amount of time and then launches
            /// </summary>
            public void StartLaunchSequence()
            {
                if (m_phase == PhaseEnum.PRETRACK)
                    StartCoroutine(LaunchSequence());
            }

            /// <summary>
            /// Starts tracking, if possible.
            /// </summary>
            public void TrackTarget()
            {
                if (m_targetState == null && m_guidanceSettings.m_target) m_targetState = new KinematicEstimator(m_guidanceSettings.m_target);

                if (m_phase == PhaseEnum.PRETRACK) m_phase = PhaseEnum.TRACKING;
            }

            /// <summary>
            /// Launches, if possible.
            /// </summary>
            /// <param name="forceLaunch">If true: Forces the sequence to launch mode. If false: identical to Launch()</param>
            public void Launch(bool forceLaunch)
            {
                if (forceLaunch && (m_phase == PhaseEnum.PRETRACK)) m_phase = PhaseEnum.TRACKING;

                Launch();
            }

            /// <summary>
            /// Launches if possible.
            /// </summary>
            public void Launch()
            {
                if (m_targetState == null && m_guidanceSettings.m_target) m_targetState = new KinematicEstimator(m_guidanceSettings.m_target);

                if (m_phase == PhaseEnum.TRACKING)
                {
                    SetGuidance();
                    if (m_guidance == null) return;
                    ResetMotor();
                    if (m_motor == null) return;

                    m_interceptStatus = InterceptEnum.NONE;
                    m_phase = PhaseEnum.FLIGHT;
                    m_hasMoved = false;
                    m_hasCheckedIntercept = false;
                    m_hasHit = false;

                    //Some launch types require some speed. This small speed will not change behavior but will make sure no numerical issues occur.
                    m_launchSpeed = MathHelp.LimitFloor(m_launchSpeed, 3f * float.Epsilon);

                    Quaternion rot = transform.rotation;
                    Vector3 boostVel = rot * Vector3.forward * m_launchSpeed;
                    m_rb.AddForce(boostVel, ForceMode.VelocityChange);

                    m_launchTime = Time.time;
                    m_launchEvent.Invoke();
                }
            }

            private void SetGuidance()
            {
                m_rb.angularDrag = 0;//Do not use the angular drag simulation in Unity, it wouldn't make any sense in this context.
                m_rb.useGravity = false;//All guidance would fully compensate for this (maneouver limitations do not account for gravity which is a simplification that has been made)
                m_rb.isKinematic = false;//Obviously...

                m_guidance = MissileGuidance.GetNewInstance(missile: this.gameObject, settings: m_guidanceSettings);
            }

            /// <summary>
            /// Sets the active motor stage.
            /// </summary>
            /// <param name="stage">Stage number. 0 is the first stage</param>
            public void SetStageNo(int stage)
            {
                int nextStage = Mathf.Clamp(stage, 0, m_motorStages.Count - 1);

                m_currentMotorStage = nextStage - 1;
                NextStage();
            }

            private void ResetMotor()
            {
                SetStageNo(0);
            }

            private void NextStage()
            {
                if (m_motorStages.Count < 1) m_motorStages.Add(new ThrustControl.MotorSettings());

                m_currentMotorStage++;

                m_currentMotorStage = Mathf.Clamp(m_currentMotorStage, 0, m_motorStages.Count - 1);

                if (m_motor != null) m_motor.m_endFlightTimeEvent.RemoveListener(NextStage);//Should not be needed, but done anyway just to be sure.

                var stage = m_motorStages[m_currentMotorStage];
                m_motor = ThrustControl.GetNewInstance(missile: this.gameObject, settings: stage);

                if (m_motor != null) m_motor.m_endFlightTimeEvent.AddListener(NextStage);
            }

            private float m_lastInterceptDir = default;
            private Vector3 m_lastInterceptMissilePos = default;
            private Vector3 m_lastInterceptTargetPos = default;
            private bool m_hasCheckedIntercept = false;
            private bool m_hasHit = false;

            private InterceptEnum CheckIntercept()
            {
                if (!m_guidance.AimingForTarget()) return m_interceptStatus;//This happens when following path and is not on the last leg/waypoint

                Vector3 targetPos;
                Vector3 targetVel;

                m_guidance.GetTargetState(out targetPos, out targetVel);

                Vector3 R = targetPos - transform.position;//Vector from missile to target
                Vector3 Vr = targetVel - m_rb.velocity;//The velocity of the missile towards the target
                var interceptDir = Vector3.Dot(R, Vr);//The sign of this indicates wether the missile is going towards the target

                if (m_hasCheckedIntercept && m_phase != PhaseEnum.DONE)//Todo: also check if intercept on last point in path (changes other places too...)
                {
                    //Todo: check if going from and within interceptrange
                    if ((interceptDir >= 0) && (m_lastInterceptDir < 0))
                    {
                        float interceptRange;

                        if (m_hasCheckedIntercept)
                        {
                            Vector3 relLastTargetPos = m_lastInterceptTargetPos - m_lastInterceptMissilePos;
                            Vector3 relCurrentTargetPos = targetPos - this.transform.position;

                            //Note that DistancePointToLineSegment clamps the segment. Shouldnt matter here since it should clamp (?) and the real distance isnt really needed.
                            interceptRange = VectorCalculation.DistancePointToLineSegment(relLastTargetPos, relCurrentTargetPos, Vector3.zero);
                        }
                        else interceptRange = R.magnitude;

                        if (interceptRange <= m_interceptRange)
                        {
                            if (!m_hasHit) m_interceptEvent.Invoke(this.gameObject);//Only hit once. Todo: return hitpoint from interceptRange above instead
                            m_hasHit = true;
                            m_interceptStatus = InterceptEnum.INRANGE;
                        }
                        else
                        {
                            m_interceptStatus = InterceptEnum.OUTSIDERANGE;
                        }
                    }
                }

                m_lastInterceptDir = interceptDir;
                m_hasCheckedIntercept = true;
                m_lastInterceptMissilePos = transform.position;
                m_lastInterceptTargetPos = targetPos;

                return m_interceptStatus;
            }

            void OnDrawGizmos()
            {
                if (!m_debug) return;

                if (m_guidanceSettings.m_targetType == MissileGuidance.TargetType.PATH)
                {
                    var line = m_guidanceSettings.GetPathPositions();

                    for (int i = 0; i < line.Count - 1; i++)
                    {
                        var pos = line[i];
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(pos, line[i + 1]);
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(pos, pos - new Vector3(0, pos.y, 0));
                    }

                    if (line.Count >= 2)
                    {
                        var pos = line[line.Count - 1];

                        Gizmos.DrawSphere(pos, 0.02f);
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(pos, pos - new Vector3(0, pos.y, 0));
                        Gizmos.color = Color.red;
                        if (m_guidanceSettings.m_loopPath) Gizmos.DrawLine(pos, line[0]);
                    }
                }
            }
        }
    }
}