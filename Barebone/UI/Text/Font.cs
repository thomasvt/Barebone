using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Barebone.Graphics;
using Barebone.Graphics.Gpu;
using BareBone.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace Barebone.UI.Text
{
    // how text is rendered: http://www.angelcode.com/products/bmfont/doc/render_text.html

    /// <summary>
    /// A more advanced font renderer. Supports variable size glyphs and kerning. Can define fonts by importing BMFont files, or your own implementation using the ctor.
    /// </summary>
    public class Font
    {
        public ITexture Texture { get; }

        /// <summary>
        /// The height of a text-line in this font in pixels.
        /// </summary>
        public readonly int LineHeight;
        internal readonly int Base;
        internal readonly Dictionary<int, Glyph> Glyphs;
        private readonly Dictionary<uint, int> _kernings; // kerning-distance by First+Second charcode combined into uint.

        /// <param name="lineHeight">The total height of a line, aka how many pixels are between text lines.</param>
        /// <param name="base">The vertical offset from the top of the baseline of the text (where the bottom of the text aligns to) aka the bottom of tail-less characters.</param>
        public Font(ITexture texture, int lineHeight, int @base, IEnumerable<Glyph> glyphs, IEnumerable<Kerning> kernings)
        {
            Texture = texture;
            LineHeight = lineHeight;
            Base = @base;
            Glyphs = glyphs.ToDictionary(g => g.Code);
            _kernings = kernings.ToDictionary(k => (uint)(k.First << 16) | (uint)k.Second, k => k.Amount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetKerningOffset(ushort first, ushort second)
        {
            var mask = (uint) (first << 16) | second;
            return _kernings.GetValueOrDefault(mask, 0);
        }

        /// <summary>
        /// Creates a <see cref="Font"/> for the generated output from the BMFont tool. The Font will include the `pngTexture` altered for rendering.
        /// </summary>
        public static Font FromBMFontFile(string fntFile, ITexture pngTexture)
        {
            if (!File.Exists(fntFile))
                throw new FileNotFoundException($"Font file should have an accompanying fnt-file '{fntFile}'. Did you use BMFont to generate?");

            return FromBMFontStream(File.OpenRead(fntFile), pngTexture);
        }

        public static Font FromBMFontStream(Stream fntStream, ITexture pngTexture)
        {
            if (new XmlSerializer(typeof(global::font)).Deserialize(fntStream) is not global::font fontDefinition)
                throw new Exception(".fnt file content is not valid. Did you use BMFont to generate?");

            var common = fontDefinition.Items.OfType<fontCommon>().First();
            var @base = int.Parse(common.@base);
            var atlasWidth = int.Parse(common.scaleW);
            var atlasHeight = int.Parse(common.scaleH);
            var lineHeight = int.Parse(common.lineHeight);

            var glyphs = ParseGlyphs(fontDefinition, atlasWidth, atlasHeight);
            var kernings = ParseKernings(fontDefinition);

            var texture = ConvertGraynessToOpacity(pngTexture);

            return new Font(texture, lineHeight, @base, glyphs, kernings);
        }

        private static ITexture ConvertGraynessToOpacity(ITexture texture)
        {
            var pixels = new GpuColor[texture.Size.X * texture.Size.Y];
            texture.ReadPixels(pixels);
            for (var i = 0; i < pixels.Length; i++)
            {
                // todo untested: had to add sidestep GpuColor -> Color -> GpuColor.
                ref var c = ref pixels[i];
                var color = c.ToColor();
                color = Color.FromArgb(color.R, 255,255, 255);
                c = color.ToGpuColor();
            }

            texture.WritePixels(pixels);
            return texture;
        }

        private static List<Kerning> ParseKernings(global::font fontDefinition)
        {
            var kerningsIn = fontDefinition.Items.OfType<fontKernings>().SingleOrDefault();
            if (kerningsIn == null)
                return new List<Kerning>();

            var kerningCount = int.Parse(kerningsIn.count);
            var kernings = new List<Kerning>(kerningCount);
            foreach (var kerning in kerningsIn.kerning)
            {
                kernings.Add(new Kerning(
                    ushort.Parse(kerning.first),
                    ushort.Parse(kerning.second),
                    int.Parse(kerning.amount)
                ));
            }

            return kernings;
        }

        private static List<Glyph> ParseGlyphs(global::font fontDefinition, int atlasWidth, int atlasHeight)
        {
            var fontChars = fontDefinition.Items.OfType<fontChars>().Single();
            var charCount = int.Parse(fontChars.count);
            var glyphs = new List<Glyph>(charCount);
            foreach (var @char in fontChars.@char)
            {
                var x = (float)int.Parse(@char.x);
                var y = (float)int.Parse(@char.y);
                var w = int.Parse(@char.width);
                var h = int.Parse(@char.height);

                glyphs.Add(new Glyph(
                    int.Parse(@char.id),
                    new Vector2(x/atlasWidth, y/atlasHeight),
                    new Vector2((x+w) / atlasWidth, (y+h) / atlasHeight),
                    w,
                    h,
                    int.Parse(@char.xoffset),
                    int.Parse(@char.yoffset),
                    int.Parse(@char.xadvance)
                ));
            }

            return glyphs;
        }

        /// <summary>
        /// Returns the size of the rendered text in pixels, but the height only measures from the top to the Base of the characters. Tails are not included. Use this as line-height, and have letter-tails penetrate into eg. margin space of a control.
        /// </summary>
        public Vector2 MeasureBase(ReadOnlySpan<char> text, float scale = 1f)
        {
            var width = 0;
            var previousCharCode = (ushort)0;
            foreach (var code in text)
            {
                if (!Glyphs.TryGetValue(code, out var glyph))
                {
                    if (!Glyphs.TryGetValue('?', out glyph))
                        continue;
                }

                width += glyph.XAdvance + GetKerningOffset(previousCharCode, code);

                previousCharCode = code;
            }

            return new Vector2(width, Base) * scale;
        }

        /// <summary>
        /// Returns the size of the rendered text in pixels.
        /// </summary>
        public Vector2 Measure(ReadOnlySpan<char> text, float scale = 1f)
        {
            var width = 0;
            var previousCharCode = (ushort)0;
            foreach (var code in text)
            {
                if (!Glyphs.TryGetValue(code, out var glyph))
                {
                    if (!Glyphs.TryGetValue('?', out glyph))
                        continue;
                }

                width += glyph.XAdvance + GetKerningOffset(previousCharCode, code);

                previousCharCode = code;
            }

            return new Vector2(width, LineHeight) * scale;
        }

        /// <summary>
        /// Appends the triangles representing the text to be rendered to your buffer without clearing it.
        /// </summary>
        public void AppendString(BBList<GpuTexTriangle> buffer, string text, Color color, Vector2 position, float scale = 1f)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            buffer.EnsureCapacity(buffer.Count + text.Length * 2);

            var gpuColor = color.ToGpuColor();

            var x = position.X;
            var y = position.Y;
            var previousCharCode = (ushort)0;
            for (var i = 0; i < text.Length; i++)
            {
                var g = System.Text.Rune.GetRuneAt(text, i);
                var code = (ushort)g.Value;
                if (!Glyphs.TryGetValue(code, out var glyph))
                {
                    if (!Glyphs.TryGetValue('?', out glyph))
                        continue;
                }

                x += GetKerningOffset(previousCharCode, code);

                var min = new Vector2(x + glyph.XOffset, y + glyph.YOffset);
                var max = min + new Vector2(glyph.Width, glyph.Heigth);

                var glyphQuad = new GlyphQuad(min, max, glyph.UVMin, glyph.UVMax);
                
                DrawGlyphQuad(buffer, glyphQuad, scale, gpuColor);

                x += glyph.XAdvance * scale;

                previousCharCode = code;
            }
        }

        /// <summary>
        /// Appends a single character. Returns how much your cursor should advance in X to position for a subsequent character.
        /// </summary>
        /// <param name="unicodePrevious">The unicode of the previous character for kerning. Pass in 0 if there isn't a previous char or you don't want kerning.</param>
        public float AppendUnicode(BBList<GpuTexTriangle> buffer, in ushort unicodePrevious, in ushort unicode, in GpuColor color, Vector2 position, in float scale = 1)
        {
            if (!Glyphs.TryGetValue(unicode, out var glyph))
            {
                if (!Glyphs.TryGetValue('?', out glyph))
                    return 0;
            }

            var x = position.X;
            var y = position.Y;
            if (unicodePrevious > 0)
                x = GetKerningOffset(unicodePrevious, unicode);

            var min = new Vector2(x + glyph.XOffset, y + glyph.YOffset);
            var max = min + new Vector2(glyph.Width, glyph.Heigth);

            var glyphQuad = new GlyphQuad(min, max, glyph.UVMin, glyph.UVMax);

            DrawGlyphQuad(buffer, glyphQuad, scale, color);

            return glyph.XAdvance * scale;
        }

        private static void DrawGlyphQuad(in BBList<GpuTexTriangle> buffer, in GlyphQuad glyphQuad, in float scale, in GpuColor color)
        {
            var left = glyphQuad.QuadMin.X * scale;
            var top = glyphQuad.QuadMin.Y * scale;
            var right = glyphQuad.QuadMax.X * scale;
            var bottom = glyphQuad.QuadMax.Y * scale;

            var uMin = glyphQuad.UVMin.X;
            var vMin = glyphQuad.UVMin.Y;
            var uMax = glyphQuad.UVMax.X;
            var vMax = glyphQuad.UVMax.Y;

            var a = new GpuTexVertex(new Vector3(left, top, 0f), color, new Vector2(uMin, vMin));
            var b = new GpuTexVertex(new Vector3(right, top, 0f), color, new Vector2(uMax, vMin));
            var c = new GpuTexVertex(new Vector3(right, bottom, 0f), color, new Vector2(uMax, vMax));
            var d = new GpuTexVertex(new Vector3(left, bottom, 0f), color, new Vector2(uMin, vMax));

            buffer.Add(new(a, b, c));
            buffer.Add(new(a, c, d));
        }

        public void Dispose()
        {
            
        }
    }
}
