using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        public class PathFollow : MissileGuidance
        {
            public PathFollow(GameObject missile, GuidanceSettings settings) : base(missile, settings)
            {
                m_pathSubset = new List<Vector3>(3);
                m_pathSubset.Add(Vector3.zero);
                m_pathSubset.Add(Vector3.zero);
            }

            private int m_lastWaypoint = 0;//The index of the waypoint in the last sample

            private bool m_firstSample = true;
            private bool m_firstLoop = true;//Is it the first loop around the path

            private List<Vector3> m_pathSubset = null;//For reuse between samples

            protected override Vector3 GetCommand()
            {
                if (m_settings.PathCount() < 1) return Vector3.zero;
                if (m_settings.PathCount() < 2)
                {
                    m_settings.m_pathObjs.Add(m_settings.m_pathObjs[0]);
                }

                Vector3 missilePos = m_missile.transform.position;
                Vector3 Vm = m_missileRb.velocity;
                float speedM = Vm.magnitude;

                var pathCount = m_settings.PathCount();
                var firstWp = m_settings.GetPathPos(0);
                var lastWp = m_settings.GetPathPos(m_settings.PathCount() - 1);

                if (m_settings.m_loopPath)
                {
                    //if looping make sure that there are at least 2 waypoints after m_lastWaypoint (and these can be the first and second wp)
                    if (m_lastWaypoint >= pathCount - 2) { pathCount++; }
                    if (m_lastWaypoint >= pathCount - 2) { pathCount++; }
                }

                m_lastWaypoint = Mathf.Clamp(m_lastWaypoint, 0, pathCount - 2);

                m_pathSubset[0] = m_settings.GetPathPosLoop(m_lastWaypoint);
                m_pathSubset[1] = m_settings.GetPathPosLoop(m_lastWaypoint + 1);

                if (m_pathSubset.Count > 2) m_pathSubset.RemoveAt(2);
                if (Mathf.Min(3, (pathCount - 1) - m_lastWaypoint + 1) > 2)
                {
                    //Not going towards next waypoint, i.e. this means that the missile is still going towards the next wp and
                    //therefore should not change to next at the first stage (which this prevents)
                    if (Vector3.Dot(Vm, missilePos - m_pathSubset[1]) > 0f)
                        m_pathSubset.Add(m_settings.GetPathPosLoop(m_lastWaypoint + 2));
                }

                int idxOrg;
                Vector3 pointOnPath;
                //First figure out which leg is current
                //Note: it is important that FindInfoClosestPointToLineSegment chooses the latest waypoint in the path if distances are identical
                VectorCalculation.FindInfoClosestPointToLineSegment(m_pathSubset, missilePos, false, out pointOnPath, out _, out idxOrg);
                var pOrg0 = m_pathSubset[idxOrg];
                var pOrg1 = m_pathSubset[idxOrg + 1];

                idxOrg += m_lastWaypoint;//m_pathSubset has m_lastWaypoint = 0

                var leg1 = pOrg1 - pOrg0;

                float minTurnRadius = 0f;//When no limit on acceleration

                //Calculate the distance before the turn for next leg should be done
                float dTurnDist = leg1.magnitude;//Never turn before the length of the leg// m_tPredTurn * Vm.magnitude;//The distance at which turn into the next leg

                float turnMarginFactor;

                turnMarginFactor = MathHelp.Remap(m_settings.m_turnMarginSimple, -1f, 0f, 1f, 0f, 1.4f, 5f);

                if (turnMarginFactor < float.Epsilon)
                {
                    dTurnDist = 0f;
                }
                else if (idxOrg <= pathCount - 1 - 2)//Only if there is one more leg after
                {
                    var pOrg2 = m_settings.GetPathPosLoop(idxOrg + 2);
                    var leg2 = (pOrg1 - pOrg2);

                    //Calculate the acceleration that the missile would do (initially) if at idxOrg + 1 going in leg1 direction to turn around to leg2
                    minTurnRadius = CalcTurnRadius(p0: pOrg0, p1: pOrg1, p2: pOrg2, speed: speedM);

                    //Calculate the distance when to start turning along the turning radius going from leg1 to leg2
                    float angleLeg1Leg2 = Vector3.Angle(leg1, leg2) * Mathf.Deg2Rad;
                    float tan2 = Mathf.Tan(angleLeg1Leg2 / 2f);
                    if (tan2 > float.Epsilon)
                    {
                        float dRadiusTurn = turnMarginFactor * minTurnRadius / tan2;
                        if (dRadiusTurn < dTurnDist) dTurnDist = dRadiusTurn;
                    }
                }

                //If the distance to the next waypoint is shorter than dTurnDist then choose the next waypoint
                //Todo: check if idxOrg + 1 has been passed
                int idx;//Indicates which leg should be active

                if (idxOrg < pathCount - 1)
                    idx = ((pOrg1 - pointOnPath).sqrMagnitude >= dTurnDist * dTurnDist) ? idxOrg : Mathf.Clamp(idxOrg + 1, 0, pathCount - 1 - 1);
                else
                    idx = idxOrg;

                if (m_settings.m_loopPath && (idx == m_settings.PathCount()))
                {
                    idx = 0;
                    m_firstLoop = false;
                }
                var p0 = m_settings.GetPathPosLoop(idx);
                var p1 = m_settings.GetPathPosLoop(idx + 1);
                Vector3 a = Vector3.zero;

                Vector3 legDir = (p1 - p0).normalized;//doesnt change if path is raised in terrain following

                pointOnPath = VectorCalculation.ClosestPointOnLineSegment(p0, p1, missilePos, false);

                //First loop and missile is placed before first waypoint then aim for the first waypoint instead of the closest point on the leg (extended back past wp)
                if ((idx == 0) && m_firstLoop && (Vector3.Dot(legDir, pointOnPath - firstWp) < 0))
                {
                    pointOnPath = firstWp;
                }

                float distLookForwardAlgorithm = speedM * m_settings.m_tTurnIn;
                float distLookForward = distLookForwardAlgorithm;
                if (m_settings.m_limitAcceleration)
                {
                    distLookForward = Mathf.Max(distLookForwardAlgorithm, speedM * speedM / m_settings.m_maxAcceleration);//The distance forward to aim for. minTurnRadius limits what is possible when limited acceleration, 0.5 for some reason, it just works better...
                }

                //Calculate the distance (on leg) required to turn into target. With some margin added.
                //Vm.magnitude * m_tTurnIn limited by algorithm
                //2*minTurnRadius limited by minumum turnradius (best case is bang-bang (2*minradius). Some margin added.)

                Vector3 refPoint;
                refPoint = pointOnPath + legDir * distLookForward;

                Vector3 R = refPoint - missilePos;

                //Calculate acceleration command
                if (R.sqrMagnitude > float.Epsilon && (speedM > float.Epsilon))
                {
                    Vector3 Rref = refPoint - pointOnPath;
                    Vector3 Vref = legDir * speedM;

                    //From http://www.diva-portal.org/smash/get/diva2:18740/FULLTEXT01.pdf
                    a += -m_settings.m_pathN * 1 / R.magnitude * Vector3.Cross(Vector3.Cross(R.normalized, Vm) - Vector3.Cross(Rref.normalized, Vref), Vm);//The second part is always zero for now (prepared for future)

                    if (m_settings.m_limitAcceleration) a = VectorCalculation.LimitMagnitude(a, m_settings.m_maxAcceleration);
                }
                else a = Vector3.zero;

                if (m_lastWaypoint != idx || m_firstSample) m_eventNextWayPoint.Invoke(idx);

                m_lastWaypoint = idx;
                m_firstSample = false;
                return a;
            }

            override public bool AimingForTarget()
            {
                return false;
            }

            override public void GetTargetState(out Vector3 pos, out Vector3 vel)
            {
                vel = Vector3.zero;//A waypoint is not moving

                if (m_settings.m_pathObjs != null && m_settings.PathCount() > 0)
                {
                    pos = m_settings.m_pathObjs[m_settings.PathCount() - 1].transform.position;
                }
                else
                {
                    pos = Vector3.zero;
                }
            }

            private float CalcTurnRadius(Vector3 p0, Vector3 p1, Vector3 p2, float speed)
            {
                var leg1 = (p1 - p0);
                var leg2 = (p1 - p2);
                var leg1Norm = leg1.normalized;
                var leg2Norm = leg2.normalized;
                var distForward = speed * m_settings.m_tTurnIn;
                //Calculate the acceleration that the missile would do (initially) if at p1 going in leg1 direction to turn around to leg2
                //This is used for the calculation of the turnradius
                var toNextWp = leg1Norm * distForward;

                //Identical to below (with simplifications)
                float turnAroundAcc = (m_settings.m_pathN / (m_settings.m_tTurnIn * m_settings.m_tTurnIn) * Vector3.Cross(toNextWp, leg2Norm)).magnitude;
                float minTurnRadius = (turnAroundAcc > float.Epsilon) ? speed * speed / turnAroundAcc : float.MaxValue;

                if (m_settings.m_limitAcceleration) minTurnRadius = Mathf.Max(minTurnRadius, speed * speed / m_settings.m_maxAcceleration);
                return minTurnRadius;
            }
        }
    }
}