using System.Drawing;
using System.Runtime.InteropServices;

namespace Barebone.Graphics
{
    public record struct ColorF(float R, float G, float B, float A = 1f)
    {
        public static ColorF FromColor(in Color c)
        {
            return new ColorF(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public record struct ColorRgba(byte R, byte G, byte B, byte A)
    {
        public static ColorRgba FromColor(in Color c)
        {
            return new ColorRgba(c.R, c.G, c.B, c.A);
        }
    }
}
