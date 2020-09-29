using System;
using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    public class MathHelp
    {
        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180.0 / Math.PI) * radians;
            return (degrees);
        }


        public static double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180.0) * degrees;
            return (radians);
        }

        public static double RadiansToPiToMinusPi(double Angle)
        {
            double CorrectedAngles = Angle % (Math.PI * 2.0);

            if (CorrectedAngles > Math.PI || CorrectedAngles < -Math.PI)
            {
                CorrectedAngles -= Math.Sign(CorrectedAngles) * 2.0 * Math.PI;
            }

            return (CorrectedAngles);
        }

        public static Vector3 RadiansToPiToMinusPi(Vector3 Angle)
        {
            Vector3 v = new Vector3
            {
                x = RadiansToPiToMinusPi(Angle.x),
                y = RadiansToPiToMinusPi(Angle.y),
                z = RadiansToPiToMinusPi(Angle.z)
            };
            return v;
        }

        public static double DegreesTo180ToMinus180(double Degrees)
        {
            double CorrectedAngles = Degrees % (360.0);

            if (CorrectedAngles > 180.0 || CorrectedAngles < -180)
            {
                CorrectedAngles -= Math.Sign(CorrectedAngles) * 360.0;
            }

            return (CorrectedAngles);
        }

        public static Vector3 DegreesTo180ToMinus180(Vector3 Degrees)
        {
            Vector3 v = new Vector3
            {
                x = DegreesTo180ToMinus180(Degrees.x),
                y = DegreesTo180ToMinus180(Degrees.y),
                z = DegreesTo180ToMinus180(Degrees.z)
            };
            return v;
        }

        public static float Diminishing(float u, float limit, float factor)
        {
            if (u >= limit)
            {
                return 0f;
            }
            float d = limit - u;
            float y = 1f / Mathf.Pow(1f + 1 / factor, 1f / d);
            return y;
        }

        public static float Diminishing(float u, float limit)
        {
            return Diminishing(u, limit, 10f);
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// Value is not clamped, i.e. linear extrapolation is done ouside range.
        /// </summary>
        /// <param name="u">Value to remap</param>
        /// <param name="xmin">Start of original range</param>
        /// <param name="xmax">End of original range</param>
        /// <param name="ymin">Start of new range</param>
        /// <param name="ymax">End of new range</param>
        /// <returns></returns>
        public static float Remap(float u, float xmin, float xmax, float ymin, float ymax)
        {
            float dx = xmax - xmin;
            if (dx < float.Epsilon) return u;

            return ymin + (u - xmin) * (ymax - ymin) / dx;
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// The ranges include a pivot. This means that there is actually two ranges that can be mapped:
        /// [xmin, xpivot] => [ymin, ypivot] and
        /// [xpivot, xmax] => [ypivot, ymax].
        /// Value is not clamped, i.e. linear extrapolation is done ouside ranges.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="xmin">End of original range</param>
        /// <param name="xpivot">Pivot in original range</param>
        /// <param name="xmax">Start of original range</param>
        /// <param name="ymin">Start of new range</param>
        /// <param name="ypivot">Pivot in new range</param>
        /// <param name="ymax">End of new range</param>
        /// <returns></returns>
        public static float Remap(float u, float xmin, float xpivot, float xmax, float ymin, float ypivot, float ymax)
        {
            if (u < xpivot)
            {
                return Remap(u, xmin, xpivot, ymin, ypivot);
            }
            else
            {
                return Remap(u, xpivot, xmax, ypivot, ymax);
            }
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// Value can be clamped.
        /// </summary>
        /// <param name="u">Value to remap</param>
        /// <param name="xmin">Start of original range</param>
        /// <param name="xmax">End of original range</param>
        /// <param name="ymin">Start of new range</param>
        /// <param name="ymax">End of new range</param>
        /// <param name="clamp">Clamp resulting value to range [ymin, ymax] if true</param>
        /// <returns></returns>
        public static float Remap(float u, float xmin, float xmax, float ymin, float ymax, bool clamp)
        {
            float y = Remap(u, xmin, xmax, ymin, ymax);
            return clamp ? Mathf.Clamp(y, ymin, ymax) : y;
        }

        public static float ConvertDegreesToRadians(float radians) => Convert.ToSingle(ConvertDegreesToRadians((double)radians));
        public static float ConvertRadiansToDegrees(float radians) => Convert.ToSingle(ConvertRadiansToDegrees((double)radians));
        public static float RadiansToPiToMinusPi(float radians) => Convert.ToSingle(RadiansToPiToMinusPi((double)radians));
        public static float DegreesTo180ToMinus180(float Degrees) => Convert.ToSingle(DegreesTo180ToMinus180((double)Degrees));

        public static float LimitCeil(float u, float ceil) { return (u <= ceil) ? u : ceil; }
        public static float LimitFloor(float u, float ceil) { return (u >= ceil) ? u : ceil; }

        public static int LimitCeil(int u, int ceil) { return (u <= ceil) ? u : ceil; }
        public static int LimitFloor(int u, int ceil) { return (u >= ceil) ? u : ceil; }

        public enum InterpolationType
        {
            NEAREST = 0,
            NEXT = 1,
            PREVIOUS = 2,
            LINEAR = 3
        }

        /// <summary>
        /// Interpolate list u. Extrapolation is done.
        /// Returns y-value at x from interpolated Vector2 list.
        /// See Interpolate(List<Vector2> u, float x, InterpolationType type, bool extrapolate).
        /// </summary>
        public static float Interpolate(List<Vector2> u, float x, InterpolationType type) => Interpolate(u: u, x: x, type: type, extrapolate: true);

        /// <summary>
        /// Interpolate list u.
        /// Returns y-value at x from interpolated Vector2 list.
        /// </summary>
        /// <param name="u">List of values to interpolate from. x-values are sample points and y-values corresponding values.</param>
        /// <param name="x"Query point at which interpolation is done.></param>
        /// <param name="type">Type of interpolation.</param>
        /// <param name="extrapolate">Extrapolation outside range sample points in u. If no interpolation x is clamped within range.</param>
        /// <returns>Interpolated value at x.</returns>
        public static float Interpolate(List<Vector2> u, float x, InterpolationType type, bool extrapolate)
        {
            //Todo: Sort u

            if (u.Count < 1) return 0f;
            if (u.Count < 2) return u[0].y;

            if (!extrapolate)
            {
                x = Mathf.Clamp(x, u[0].x, u[u.Count - 1].x);
            }

            //Todo: Optimize finding intervalStart
            int intervalStart = 0;
            for (int i = 1; i < u.Count - 1; i++)
            {
                if (x < u[i].x) break;
                intervalStart = i;
            }
            float y;
            switch (type)
            {
                case InterpolationType.NEAREST:
                    y = (x < (u[intervalStart].x + u[intervalStart + 1].x) / 2f) ? u[intervalStart].y : u[intervalStart + 1].y;
                    break;
                case InterpolationType.NEXT:
                    y = u[intervalStart + 1].y;
                    break;
                case InterpolationType.PREVIOUS:
                    y = x < u[u.Count - 1].x ? u[intervalStart].y : u[u.Count - 1].y;
                    break;
                case InterpolationType.LINEAR:
                default:
                    float dx = u[intervalStart + 1].x - u[intervalStart].x;
                    float dy = u[intervalStart + 1].y - u[intervalStart].y;
                    y = (dx > float.Epsilon) ? u[intervalStart].y + (x - u[intervalStart].x) / dx * dy : u[intervalStart].y;
                    break;
            }
            return y;
        }
    }
}
