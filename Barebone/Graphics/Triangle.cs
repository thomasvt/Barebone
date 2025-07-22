using System.Drawing;
using System.Numerics;

namespace Barebone.Graphics
{
    public record struct Triangle(Vector3 A, Vector3 B, Vector3 C, Color Color)
    {
        public static Triangle operator *(Triangle t, float weight)
        {
            return new Triangle(t.A * weight, t.B * weight, t.C * weight, t.Color);
        }
        
        public static Triangle operator +(Triangle t, Vector3 b)
        {
            return new Triangle(t.A + b, t.B +b, t.C +b, t.Color);
        }
    };
}
