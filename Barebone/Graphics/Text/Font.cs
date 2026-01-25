using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Barebone.Geometry;
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
        public Font(ITexture texture, int lineHeight, int @base, IReadOnlyList<Glyph> glyphs, IEnumerable<Kerning> kernings)
        {
            Texture = texture;
            LineHeight = lineHeight;
            Base = @base;
            Glyphs = glyphs.ToDictionary(g => g.Code);
            _kernings = kernings.ToDictionary(k => (uint)(k.First << 16) | (uint)k.Second, k => k.Amount);

            DetectMonospaceFont(glyphs);
        }

        private void DetectMonospaceFont(IReadOnlyList<Glyph> glyphs)
        {
            var xAdvances = glyphs.Select(g => g.XAdvance).Distinct().ToList();
            IsMonospaceFont = xAdvances.Count() == 1;
            if (IsMonospaceFont)
                MonospaceWidth = xAdvances.Single();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetKerningOffset(ushort first, ushort second)
        {
            var mask = (uint) (first << 16) | second;
            return _kernings.GetValueOrDefault(mask, 0);
        }

        /// <summary>
        /// Creates a <see cref="Font"/> for the output from the BMFont tool. The Font will include the `pngTexture` altered for rendering.
        /// </summary>
        public static Font FromBMFontFile(string fntFile, ITexture pngTexture)
        {
            if (!File.Exists(fntFile))
                throw new FileNotFoundException($"Font file should have an accompanying fnt-file '{fntFile}'. Did you use BMFont to generate?");

            return FromBMFontStream(File.OpenRead(fntFile), pngTexture);
        }

        /// <summary>
        /// Creates a <see cref="Font"/> from a string that contains the output from the BMFont tool. The Font will include the `pngTexture` altered for rendering.
        /// </summary>
        public static Font FromBMFontXml(string fntXml, ITexture pngTexture)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fntXml));
            stream.Position = 0;
            return FromBMFontStream(stream, pngTexture);
        }

        public static Font FromBMFontStream(Stream fntStream, ITexture pngTexture)
        {
            if (new XmlSerializer(typeof(global::font)).Deserialize(fntStream) is not global::font fontDefinition)
                throw new Exception(".fnt file content is not valid. Did you use BMFont to generate it?");

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
        /// Returns the size of the rendered text in texels, but the height only measures from the top to the Base of the characters. Tails are not included. Use this as line-height, and have letter-tails penetrate into eg. margin space of a control.
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
        /// Returns the size of the rendered text in texels.
        /// </summary>
        public Vector2I Measure(ReadOnlySpan<char> text)
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

            return new Vector2I(width, LineHeight);
        }

        /// <summary>
        /// Gets the character index of the glyph-start (left-side) closest to the given `caretX`. Used for screen picking caret positions.
        /// Note that this can return text.Length if the caret is to the right of the last glyph.
        /// </summary>
        public int GetCharacterAtCaretX(ReadOnlySpan<char> text, int caretX)
        {
            var width = 0;
            var previousCharCode = (ushort)0;
            var i = 0;
            foreach (var code in text)
            {
                if (!Glyphs.TryGetValue(code, out var glyph))
                {
                    if (!Glyphs.TryGetValue('?', out glyph))
                        continue;
                }

                var nextWidth = width + glyph.XAdvance + GetKerningOffset(previousCharCode, code);
                if (caretX < nextWidth)
                {
                    if (caretX - width <= nextWidth - caretX)
                        return i;
                    return i + 1;
                }
                width = nextWidth;
                previousCharCode = code;
                i++;
            }

            return text.Length;
        }

        /// <summary>
        /// Appends the triangles representing the text to be rendered to your buffer without clearing it.
        /// </summary>
        public void AppendString(bool yPointsDown, BBList<GpuTexTriangle> buffer, string text, Color color, in Vector2 position, float scale = 1f, in float z = 0)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            buffer.EnsureCapacity(buffer.Count + text.Length * 2);

            var gpuColor = color.ToGpuColor();

            var x = 0f;
            var y = 0f;
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
                
                DrawGlyphQuad(yPointsDown, buffer, position, glyphQuad, scale, gpuColor, z);

                x += glyph.XAdvance;

                previousCharCode = code;
            }
        }

        /// <summary>
        /// Appends a single character. Returns how much your cursor should advance in X to position for a subsequent character.
        /// </summary>
        /// <param name="unicodePrevious">The unicode of the previous character for kerning. Pass in 0 if there isn't a previous char or you don't want kerning.</param>
        public float AppendUnicode(in bool yPointsDown, BBList<GpuTexTriangle> buffer, in ushort unicodePrevious, in ushort unicode, in GpuColor color, in Vector2 position, in float scale = 1, in float z = 0)
        {
            if (!Glyphs.TryGetValue(unicode, out var glyph))
            {
                if (!Glyphs.TryGetValue('?', out glyph))
                    return 0;
            }

            var x = 0;
            var y = 0;
            if (unicodePrevious > 0)
                x = GetKerningOffset(unicodePrevious, unicode);

            var min = new Vector2(x + glyph.XOffset, y + glyph.YOffset);
            var max = min + new Vector2(glyph.Width, glyph.Heigth);

            var glyphQuad = new GlyphQuad(min, max, glyph.UVMin, glyph.UVMax);

            DrawGlyphQuad(yPointsDown, buffer, position, glyphQuad, scale, color, z);

            return glyph.XAdvance;
        }

        private static void DrawGlyphQuad(in bool yPointsDown, in BBList<GpuTexTriangle> buffer, in Vector2 position, in GlyphQuad glyphQuad, in float scale, in GpuColor color, in float z = 0)
        {
            if (yPointsDown)
            {
                var left = position.X + glyphQuad.QuadMin.X * scale;
                var top = position.Y + glyphQuad.QuadMin.Y * scale;
                var right = position.X + glyphQuad.QuadMax.X * scale;
                var bottom = position.Y + glyphQuad.QuadMax.Y * scale;

                var uMin = glyphQuad.UVMin.X;
                var vMin = glyphQuad.UVMin.Y;
                var uMax = glyphQuad.UVMax.X;
                var vMax = glyphQuad.UVMax.Y;

                var a = new GpuTexVertex(new Vector3(left, top, z), color, new Vector2(uMin, vMin));
                var b = new GpuTexVertex(new Vector3(right, top, z), color, new Vector2(uMax, vMin));
                var c = new GpuTexVertex(new Vector3(right, bottom, z), color, new Vector2(uMax, vMax));
                var d = new GpuTexVertex(new Vector3(left, bottom, z), color, new Vector2(uMin, vMax));

                buffer.Add(new(a, b, c));
                buffer.Add(new(a, c, d));
            }
            else
            {
                var left = position.X + glyphQuad.QuadMin.X * scale;
                var top = position.Y - glyphQuad.QuadMin.Y * scale;
                var right = position.X + glyphQuad.QuadMax.X * scale;
                var bottom = position.Y - glyphQuad.QuadMax.Y * scale;

                var uMin = glyphQuad.UVMin.X;
                var vMin = glyphQuad.UVMin.Y;
                var uMax = glyphQuad.UVMax.X;
                var vMax = glyphQuad.UVMax.Y;

                var a = new GpuTexVertex(new Vector3(left, top, z), color, new Vector2(uMin, vMin));
                var b = new GpuTexVertex(new Vector3(right, top, z), color, new Vector2(uMax, vMin));
                var c = new GpuTexVertex(new Vector3(right, bottom, z), color, new Vector2(uMax, vMax));
                var d = new GpuTexVertex(new Vector3(left, bottom, z), color, new Vector2(uMin, vMax));

                buffer.Add(new(a, b, c));
                buffer.Add(new(a, c, d));
            }
        }

        /// <summary>
        /// Width of a single character of this font, if it's monospace.
        /// </summary>
        public int? MonospaceWidth { get; set; }

        /// <summary>
        /// Autodetected to True if widths taken by all characters of this font are the same.
        /// </summary>
        public bool IsMonospaceFont { get; set; }
    }
}
