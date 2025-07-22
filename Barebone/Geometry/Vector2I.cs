using System.Numerics;
using System.Text.Json.Serialization;

namespace Barebone.Geometry
{
    public readonly record struct Vector2I(int X, int Y)
    {
        [JsonIgnore] public int ManhattanLength => Math.Abs(X) + Math.Abs(Y);

        public Vector2I(int xy) : this(xy, xy)
        {
        }

        public static Vector2 operator *(Vector2I a, float b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }

        public static Vector2I operator *(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2 operator /(Vector2I a, float b)
        {
            return new Vector2(a.X / b, a.Y / b);
        }

        public static Vector2I operator *(Vector2I a, int b)
        {
            return new Vector2I(a.X * b, a.Y * b);
        }

        public static Vector2I operator /(Vector2I a, int b)
        {
            return new Vector2I(a.X / b, a.Y / b);
        }

        public static Vector2I operator %(Vector2I a, int b)
        {
            return new Vector2I(a.X % b, a.Y % b);
        }

        public static Vector2I operator +(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2I operator -(Vector2I a)
        {
            return new Vector2I(-a.X, -a.Y);
        }

        public static Vector2I operator -(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator +(Vector2I a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2I a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static implicit operator Vector2(Vector2I v)
        {
            return new Vector2(v.X, v.Y);
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Calculates the cross product ("a×b"). Interpretable as the surface area of the parallelogram formed by the two vectors.
        /// </summary>
        public static float Cross(Vector2I a, Vector2I b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }

        public Vector3 ToVector3(float z = 0)
        {
            return new Vector3(X, Y, z);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public static Vector2I Zero = new(0, 0);
        public static Vector2I One = new(1, 1);
    }
}
