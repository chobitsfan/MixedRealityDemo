using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    public class VectorCalculation
    {
        /// <summary>
        /// Calculates the vector from a point to a line.
        /// The line is descriped as lineStart + t * lineDirection
        /// See https://en.m.wikipedia.org/wiki/Distance_from_a_point_to_a_line
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineDirection"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 PointToLine(Vector3 lineStart, Vector3 lineDirection, Vector3 point)
        {
            //Vector3 n = lineDirection.normalized;
            Vector3 n = lineDirection;//.normalized;
            Vector3 relVector = lineStart - point;
            //Vector3 p = point;
            //Vector3 a = lineStart;

            float nSqrMag = n.sqrMagnitude;
            if (nSqrMag < float.Epsilon) return relVector;

            var dVec = relVector - Vector3.Dot(relVector, n) * n / nSqrMag;
            //var dVec = (a - p) - (Vector3.Dot((a - p), n)) * n;

            return dVec;
        }

        /// <summary>
        /// The point on a line segment that is closest to a point
        /// </summary>
        /// <param name="segmentStart"></param>
        /// <param name="segmentEnd"></param>
        /// <param name="point"></param>
        /// <param name="clamped">Is the result clamped between segment start and end</param>
        /// <returns></returns>
        public static Vector3 ClosestPointOnLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point, bool clamped)
        {
            var segVec = (segmentEnd - segmentStart);

            var closestPointOnLine = point + VectorCalculation.PointToLine(segmentStart, segVec, point);
            if (clamped)
            {
                var segLengthSqr = segVec.sqrMagnitude;
                if (segLengthSqr < float.Epsilon) return segmentStart;

                var l = Vector3.Dot(segVec, closestPointOnLine - segmentStart) / segLengthSqr;
                if (l < 0)
                    return segmentStart;
                else if (l > 1)
                    return segmentEnd;
                else
                    return closestPointOnLine;
            }
            else
            {
                return closestPointOnLine;
            }
        }
        /// <summary>
        /// Same as ClosestPointOnLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point, bool clamped)
        /// clamped set to true;
        /// </summary>
        /// <param name="segmentStart"></param>
        /// <param name="segmentEnd"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ClosestPointOnLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point) => ClosestPointOnLineSegment(segmentStart, segmentEnd, point, true);

        /// <summary>
        /// Distance from a point to a line segment. Note that the line segment is not extrapolated.
        /// </summary>
        /// <param name="segmentStart"></param>
        /// <param name="segmentEnd"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float DistancePointToLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point)
        {
            var p = ClosestPointOnLineSegment(segmentStart, segmentEnd, point);
            return (p - point).magnitude;
        }

        /// <summary>
        /// Returns which line segment described as a list of vector2 a point is closest to.
        /// The index returned is the index of the first vector of a line segment.
        /// If two segments are at identical distance the one with the highest index is chosen.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static int FindClosestLineSegment(List<Vector3> lineSegments, Vector3 point)
        {
            if (lineSegments.Count <= 2)
            {
                return 0;
            }

            var idx = 0;
            var minDist = DistancePointToLineSegment(lineSegments[0], lineSegments[1], point);
            for (int i = 1; i < lineSegments.Count - 1; i++)
            {
                var d = DistancePointToLineSegment(lineSegments[i], lineSegments[i + 1], point);
                if (d <= minDist)
                {
                    idx = i;
                    minDist = d;
                }
            }
            return idx;
        }

        /// <summary>
        /// Gets info on the point on a line that is closest to a point
        /// </summary>
        /// <param name="lineSegments"></param>
        /// <param name="point"></param>
        /// <param name="closestPoint">The closest point on the line</param>
        /// <param name="closestPointDir">The normalized direction of the line at the closest point, normalized</param>
        public static void FindInfoClosestPointToLineSegment(List<Vector3> lineSegments, Vector3 point, bool clamped, out Vector3 closestPoint, out Vector3 closestPointDir, out int idx)
        {
            idx = FindClosestLineSegment(lineSegments, point);

            if (lineSegments.Count == 0)
            {
                closestPoint = Vector3.zero;
                closestPointDir = Vector3.right;
                return;
            }
            else if (lineSegments.Count == 1)
            {
                closestPoint = lineSegments[0];
                closestPointDir = Vector3.right;
                return;
            }
            //Todo: optimize. ClosestPointOnLineSegment is done twice on the chosen linesegment

            closestPoint = ClosestPointOnLineSegment(lineSegments[idx], lineSegments[idx + 1], point, clamped);
            closestPointDir = (lineSegments[idx + 1] - lineSegments[idx]).normalized;
        }
        public static void FindInfoClosestPointToLineSegment(List<Vector3> lineSegments, Vector3 point, bool clamped, out Vector3 closestPoint, out Vector3 closestPointDir) => FindInfoClosestPointToLineSegment(lineSegments, point, clamped, out closestPoint, out closestPointDir, out _);
        public static void FindInfoClosestPointToLineSegment(List<Vector3> lineSegments, Vector3 point, out Vector3 closestPoint, out Vector3 closestPointDir) => FindInfoClosestPointToLineSegment(lineSegments, point, true, out closestPoint, out closestPointDir, out _);

        public static Vector3 LimitMagnitude(Vector3 u, float maxMagnitude)
        {
            if (u.sqrMagnitude > maxMagnitude * maxMagnitude)
            {
                return u.normalized * maxMagnitude;
            }
            else
            {
                return u;
            }
        }

        public static Vector2 LimitMagnitude(Vector2 u, float maxMagnitude)
        {
            if (u.sqrMagnitude > maxMagnitude * maxMagnitude)
            {
                return u.normalized * maxMagnitude;
            }
            else
            {
                return u;
            }
        }

        /// <summary>
        /// Removes normal components from a vector.
        /// In other words, returns the position on a plane that is closest to a point
        /// </summary>
        /// <param name="normal">Normal of plane</param>
        /// <param name="origo">Origo of plane, i.e. any point on the plane</param>
        /// <param name="point">The point to project to the plane</param>
        /// <returns></returns>
        public static Vector3 RemoveNormalComponent(Vector3 normal, Vector3 origo, Vector3 point)
        {
            var sqrMagNormal = normal.sqrMagnitude;
            if (sqrMagNormal < float.Epsilon) return point;
            var corr = normal * Vector3.Dot(normal, point - origo) / sqrMagNormal;//The component of command that is not orthogonal 

            return point - corr;
        }

        /// <summary>
        /// Removes normal components from a vector.
        /// In other words, returns the position on a plane that is closest to a point. Normal and point is assumed to have the same origin.
        /// </summary>
        /// <param name="normal">Normal of plane</param>
        /// <param name="vector">The point to project to the plane</param>
        /// <returns></returns>
        public static Vector3 RemoveNormalComponent(Vector3 normal, Vector3 vector)
        {
            var sqrMagNormal = normal.sqrMagnitude;
            if (sqrMagNormal < float.Epsilon) return vector;
            var corr = normal * Vector3.Dot(normal, vector) / sqrMagNormal;//The component of command that is not orthogonal 

            return vector - corr;
        }
    }
}