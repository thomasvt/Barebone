using System.Drawing;

namespace Barebone.Graphics
{
    public record struct ColorF(float R, float G, float B, float A = 1f)
    {
        public static ColorF FromColor(in Color c)
        {
            return new ColorF(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
    }
}
