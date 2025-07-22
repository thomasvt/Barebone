using System.Drawing;

namespace Barebone.Graphics;

public readonly record struct ColorHSL(byte H, byte S, byte L)
{
    // from .NET WinForms source: https://referencesource.microsoft.com/#system.windows.forms/winforms/managed/system/winforms/ControlPaint.cs,ddacc8aa9c0a78e6

    private const int Range = 240;
    private const int HSLMax = Range;
    private const int RGBMax = 255;
    private const int Undefined = HSLMax * 2 / 3;

    public ColorHSL(Color color) : this(0, 0, 0)
    {
        int r = color.R;
        int g = color.G;
        int b = color.B;

        /* calculate lightness */
        var max = Math.Max(Math.Max(r, g), b);
        var min = Math.Min(Math.Min(r, g), b);
        var sum = max + min;

        var l = (sum * HSLMax + RGBMax) / (2 * RGBMax);
        int s;
        int h;

        var dif = max - min;
        if (dif == 0)
        {
            /* r=g=b --> achromatic case */

            s = 0;
            h = Undefined;
        }
        else
        {
            /* chromatic case */

            /* saturation */
            if (l <= HSLMax / 2)
                s = (dif * HSLMax + sum / 2) / sum;
            else
                s = (dif * HSLMax + (2 * RGBMax - sum) / 2) / (2 * RGBMax - sum);

            /* hue */
            var Rdelta = ((max - r) * (HSLMax / 6) + dif / 2) / dif;
            var Gdelta = ((max - g) * (HSLMax / 6) + dif / 2) / dif;
            var Bdelta = ((max - b) * (HSLMax / 6) + dif / 2) / dif;

            if (r == max)
                h = Bdelta - Gdelta;
            else if (g == max)
                h = HSLMax / 3 + Rdelta - Bdelta;
            else /* B == cMax */
                h = 2 * HSLMax / 3 + Gdelta - Rdelta;

            if (h < 0)
                h += HSLMax;
            if (h > HSLMax)
                h -= HSLMax;
        }

        H = (byte)h;
        S = (byte)s;
        L = (byte)l;
    }

    public Color ToColor()
    {
        byte r, g, b;                      /* RGB component values */

        if (S == 0)
        {
            /* achromatic case */
            r = g = b = (byte)(L * RGBMax / HSLMax);
            if (H != Undefined)
            {
                /* ERROR */
            }
        }
        else
        {
            /* chromatic case */

            /* set up magic numbers */
            int magic2;
            if (L <= HSLMax / 2)
                magic2 = (L * (HSLMax + S) + HSLMax / 2) / HSLMax;
            else
                magic2 = L + S - (L * S + HSLMax / 2) / HSLMax;

            var magic1 = 2 * L - magic2;       /* calculated magic numbers (really!) */

            /* get RGB, change units from HSLMax to RGBMax */
            r = (byte)((HueToRgb(magic1, magic2, H + HSLMax / 3) * RGBMax + HSLMax / 2) / HSLMax);
            g = (byte)((HueToRgb(magic1, magic2, H) * RGBMax + HSLMax / 2) / HSLMax);
            b = (byte)((HueToRgb(magic1, magic2, H - HSLMax / 3) * RGBMax + HSLMax / 2) / HSLMax);
        }
        return Color.FromArgb(r, g, b);
    }

    private static int HueToRgb(int n1, int n2, int hue)
    {
        /* range check: note values passed add/subtract thirds of range */

        /* The following is redundant for WORD (unsigned int) */
        if (hue < 0)
            hue += HSLMax;

        if (hue > HSLMax)
            hue -= HSLMax;

        return hue switch
        {
            /* return r,g, or b value from this tridrant */
            < HSLMax / 6 => n1 + ((n2 - n1) * hue + HSLMax / 12) / (HSLMax / 6),
            < HSLMax / 2 => n2,
            < HSLMax * 2 / 3 => n1 + ((n2 - n1) * (HSLMax * 2 / 3 - hue) + HSLMax / 12) / (HSLMax / 6),
            _ => n1
        };
    }

    /// <summary>
    /// Adds light to the color in a percentage within. Negative percentages will darken the color. The returned colors are capped to 255 per channel.
    /// eg. -1 will return black, +1 will double the light, +0.5 will add 50% light.
    /// 
    /// </summary>
    public ColorHSL Brighten(float pct)
    {
        pct += 1;
        return this with { L = (byte)Math.Max(0, Math.Min(255, L * pct)) };
    }

    /// <summary>
    /// Saturates the color by a percentage (or desaturates, if percentage is negative).
    /// </summary>
    public ColorHSL Saturate(float pct)
    {
        pct += 1;
        return this with { S = (byte)Math.Max(0, Math.Min(255, S * pct)) };
    }
}