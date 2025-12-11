namespace Barebone.Geometry
{
    public static class Angles
    {
        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static float ToRadians(this float degrees) => degrees * DegreeToRadians;

        public const float DegreeToRadians = MathF.PI / 180f;

        public const float _720 = MathF.Tau * 2;
        public const float _360 = MathF.Tau;
        public const float _270 = 270 * DegreeToRadians;
        public const float _240 = 240 * DegreeToRadians;
        public const float _225 = 225 * DegreeToRadians;
        public const float _180 = MathF.PI;
        public const float _135 = 135 * DegreeToRadians;
        public const float _120 = 120 * DegreeToRadians;
        public const float _090 = 90 * DegreeToRadians;
        public const float _060 = 60 * DegreeToRadians;
        public const float _045 = 45 * DegreeToRadians;
        public const float _030 = 30 * DegreeToRadians;
        public const float _022_5 = 22.5f * DegreeToRadians;
        public const float _015 = 15 * DegreeToRadians;
        public const float _010 = 15 * DegreeToRadians;
        public const float _005 = 5 * DegreeToRadians;
        public const float _003 = 3 * DegreeToRadians;
        public const float _002 = 2 * DegreeToRadians;
        public const float _001 = 1 * DegreeToRadians;
    }
}

