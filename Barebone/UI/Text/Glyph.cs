using System.Numerics;

namespace Barebone.UI.Text
{
    public readonly struct Glyph
    {
        // how text is rendered:  http://www.angelcode.com/products/bmfont/doc/render_text.html

        /// <summary>
        /// Unicode. (first 2 bytes of char.ConvertToUtf32(x))
        /// </summary>
        public readonly int Code;

        /// <summary>
        /// UV min corner on the font texture.
        /// </summary>
        public readonly Vector2 UVMin;

        /// <summary>
        /// UV max corner on the font texture.
        /// </summary>
        public readonly Vector2 UVMax;

        /// <summary>
        /// Width in texels.
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Height in texels.
        /// </summary>
        public readonly int Heigth;

        /// <summary>
        /// Extra horizontal offset from the current render position in the text to render the glyph. (position is supposed to be topleft of the glyph)
        /// </summary>
        public readonly int XOffset;

        /// <summary>
        /// Extra vertical offset from the current render position in the text to render the glyph. (position is supposed to be topleft of the glyph)
        /// </summary>
        public readonly int YOffset;

        /// <summary>
        /// How much to add to the render position after rendering the glyph to put the render position in place for the next character's glyph.
        /// </summary>
        public readonly int XAdvance;

        public Glyph(int code, Vector2 uvMin, Vector2 uvMax, int width, int heigth, int xOffset, int yOffset, int xAdvance)
        {
            Code = code;
            UVMin = uvMin;
            UVMax = uvMax;
            Width = width;
            Heigth = heigth;
            XOffset = xOffset;
            YOffset = yOffset;
            XAdvance = xAdvance;
        }
    }
}
