using System.Drawing;
using Barebone.Graphics;

namespace BareBone.Graphics
{
    public static class ColorExt
    {
        /// <summary>
        /// Convert this color to a HSL representation.
        /// </summary>
        public static ColorHSL ToHSL(this Color c) => new(c);

        /// <summary>
        /// Replaces the color, not transparency: RGB but not A.
        /// </summary>
        public static Color ReplaceRgb(this Color color, Color newRgb)
        {
            return Color.FromArgb(color.A, newRgb.R, newRgb.G, newRgb.B);
        }

        /// <summary>
        /// Parses a hex notation color, like in HTML, into a Color. Supported: "xx" (shade of gray), "rrggbb" or "rrggbbaa"
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color Parse(string hex)
        {
            hex = hex.TrimStart('#');
            var bytes = Convert.FromHexString(hex);
            if (bytes.Length == 3)
                return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
            if (bytes.Length == 4)
                return Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]);
            if (bytes.Length == 1)
                return Color.FromArgb(bytes[0], bytes[0], bytes[0]); // grayscale
            throw new FormatException($"Cannot parse '{hex}' into a {nameof(Color)}.");
        }

        public static Color ToGrayScale(this Color c)
        {
            var v = (byte)((c.R + c.G + c.B) / 3);
            return Color.FromArgb(c.A, v, v, v);
        }

        public static Color WithAlpha(this Color c, in byte alpha)
        {
            return Color.FromArgb(alpha, c);
        }

        public static Color Lerp(Color c0, Color c1, float t)
        {
            var a = (int)(c0.A * (1 - t) + c1.A * t);
            var r = (int)(c0.R * (1 - t) + c1.R * t);
            var g = (int)(c0.G * (1 - t) + c1.G * t);
            var b = (int)(c0.B * (1 - t) + c1.B * t);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
