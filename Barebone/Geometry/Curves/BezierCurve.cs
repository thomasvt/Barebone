﻿using System.Numerics;

namespace Barebone.Geometry.Curves
{
    /// <summary>
    /// Cubic Bezier curve (2 points, 2 handle points). X0 and X3 are the endpoints, X1 and X2 are the handles. A, B and C are the parameters of the 3rd grade polynomials defining fx(t) and fy(t). 
    /// </summary>
    public readonly struct BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        public readonly Vector2 P0 = p0, P1 = p1, P2 = p2, P3 = p3;

        /// <summary>
        /// Converts the curve into a chain of straigh-line segments and returns those points.
        /// </summary>
        public IEnumerable<Vector2> CalculateLineSegments(int segmentCount)
        {
            var fx = GetFx();
            var fy = GetFy();

            yield return P0;
            for (var i = 1; i < segmentCount; i++)
            {
                var t = (float) i / segmentCount;
                yield return new Vector2(fx.Solve(t), fy.Solve(t));
            }
            yield return P3;
        }

        /// <summary>
        /// Returns the polynomial representing Fx(t). 
        /// </summary>
        public Polynomial3 GetFx()
        {
            // See https://moshplant.com/direct-or/bezier/math.html
            var c = (P1.X - P0.X) * 3f;
            var b = ((P2.X - P1.X) * 3) - c;
            var a = P3.X - P0.X - c - b;
            return new Polynomial3(a, b, c, P0.X);
        }

        /// <summary>
        /// Returns the polynomial representing Fy(t).
        /// </summary>
        public Polynomial3 GetFy()
        {
            // See https://moshplant.com/direct-or/bezier/math.html
            var c = (P1.Y - P0.Y) * 3f;
            var b = ((P2.Y - P1.Y) * 3) - c;
            var a = P3.Y - P0.Y - c - b;
            return new Polynomial3(a, b, c, P0.Y);
        }

        /// <summary>
        /// Returns a bezier from (0,0) to (1,1) with both handles set to an easing value so that it can be used as a normalized easing curve for animations.
        /// </summary>
        public static BezierCurve GetEasingCurve(in float easingPercentage)
        {
            return new BezierCurve(new Vector2(0, 0), new Vector2(easingPercentage, 0), new Vector2(1f - easingPercentage, 1), new Vector2(1, 1));
        }

        /// <summary>
        /// Returns a bezier from (0,0) to (1,1) with handles set to an easing value so that it can be used as a normalized easing curve for animations.
        /// </summary>
        public static BezierCurve GetEasingCurve(in float easingInPercentage, in float easingOutPercentage)
        {
            return new BezierCurve(new Vector2(0, 0), new Vector2(easingInPercentage, 0), new Vector2(1f - easingOutPercentage, 1), new Vector2(1, 1));
        }

        /// <summary>
        /// Returns a bezier from (0,0) to (1,1) with provided bezier handles to be used in animations.
        /// </summary>
        public static BezierCurve GetEasingCurve(in Vector2 inHandle, in Vector2 outHandle)
        {
            return new BezierCurve(new Vector2(0, 0), inHandle, outHandle, new Vector2(1, 1));
        }
    }
}
