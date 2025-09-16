using System.Diagnostics.Contracts;

namespace Barebone.Geometry.Curves
{
    /// <summary>
    /// A polynomial of 2nd degree.
    /// </summary>
    public readonly record struct Polynomial2(float A, float B, float C)
    {
        /// <summary>
        /// Calculate f(t) = A*t^2 + B*t + C
        /// </summary>
        public float Calc(float t)
        {
            var t2 = t * t;
            return A * t2 + B * t + C;
        }
    }

    /// <summary>
    /// A polynomial of 3rd degree.
    /// </summary>
    public readonly record struct Polynomial3(float A, float B, float C, float D)
    {
        [Pure]
        public Polynomial2 GetDerivative()
        {
            return new Polynomial2(3f * A, 2f * B, C);
        }

        /// <summary>
        /// Calculate f(t) = A*t^3 + B*t^2 + C*t + D
        /// </summary>
        [Pure]
        public float Solve(float t)
        {
            var t2 = t * t;
            var t3 = t * t2;
            return A * t3 + B * t2 + C * t + D;
        }
    }
}
