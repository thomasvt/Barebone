using System.Drawing;
using System.Runtime.InteropServices;

namespace Barebone.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public record struct Color8(byte R, byte G, byte B, byte A)
    {
        public static Color8 FromColor(in Color c)
        {
            return new Color8(c.R, c.G, c.B, c.A);
        }

        public readonly static Color8 White = new Color8(255, 255, 255, 255);
    }
}
