using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Text;

namespace Barebone.Game.Graphics
{
    internal class GraphicsSubSystem : IGraphics, IDisposable
    {
        private readonly IPlatformGraphics _pg;
        private ITexture? _texture;
        private Matrix3x2 _uvTransform;
        private readonly Font _defaultFont;
        private readonly BBList<Vertex> _textTriangleBuffer = new();
        private ICamera _activeCamera;
        private Vector2I _viewportSize;

        public GraphicsSubSystem(IPlatformGraphics pg)
        {
            _pg = pg;
            _defaultFont = Font.FromBMFontFile("UbuntuMono32.fnt", GetTexture("UbuntuMono32_0.png"));
        }

        public void ClearScreen(in Color color)
        {
            _pg.ClearScreen(color);
        }

        public void FillPolygon(in Polygon8 polygon, in Color? color = null)
        {
            FillPolygonInternal(polygon, color ?? Color.White);
        }

        public void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color)
        {
            FillCircleInternal(center, radius, segmentCount, color);
        }

        public void Print(in Vector2 position, in string text, in Color color, in float scale = 1f)
        {
            _textTriangleBuffer.Clear();
            var colorF = ColorF.FromColor(color);
            _defaultFont.AppendString(true, _textTriangleBuffer, text, colorF, position, scale);
            foreach (ref var v in _textTriangleBuffer.AsSpan())
            {
                v.Position = Vector2.Transform(v.Position, _activeCamera.WorldToScreenTransform);
            }
            _pg.FillTriangles(_textTriangleBuffer.AsReadOnlySpan(), _defaultFont.Texture);
        }

        public void ActivateCamera(in ICamera camera)
        {
            ((Camera)camera).SetViewportSize(_viewportSize);
            _activeCamera = camera;
        }

        public void SetColorOnly()
        {
            _texture = null;
            _uvTransform = Matrix3x2.Identity;
        }

        public void SetTexture(in ITexture texture, in Matrix3x2 projection)
        {
            _texture = texture;
            _uvTransform = projection;
        }

        public ITexture GetTexture(string assetPath)
        {
            return _pg.GetTexture(assetPath);
        }

        public ICamera CreateCamera(float viewHeight, ScreenOrigin screenOrigin)
        {
            return new Camera
            {
                LogicalViewHeight = viewHeight,
                ScreenOrigin = screenOrigin
            };
        }

        public ICamera Camera => _activeCamera;

        private void FillCircleInternal(in Vector2 center, in float radius, in int segmentCount, in Color color)
        {
            var colorF = ColorF.FromColor(color);

            Span<Vertex> vertices = stackalloc Vertex[segmentCount * 3];

            var i = 0;

            var angleStep = Angles._360 / segmentCount;

            var angle = -angleStep;
            var a = center + angle.AngleToVector2(radius);

            var transform = _activeCamera.WorldToScreenTransform;

            for (var s = 0; s < segmentCount; s++)
            {
                angle += angleStep;

                var b = center + angle.AngleToVector2(radius);

                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(center, transform), UV = Vector2.Transform(center, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(a, transform), UV = Vector2.Transform(a, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(b, transform), UV = Vector2.Transform(b, _uvTransform) };

                a = b;
            }

            _pg.FillTriangles(vertices, _texture);
        }

        private void FillPolygonInternal(in Polygon8 polygon, in Color color)
        {
            var colorF = ColorF.FromColor(color);

            var pA = polygon[0];
            Span<Vertex> vertices = stackalloc Vertex[(polygon.Count - 2) * 3];

            var i = 0;
            var pB = polygon[1];

            var transform = _activeCamera.WorldToScreenTransform;

            for (var c = 2; c < polygon.Count; c++)
            {
                var pC = polygon[c]; // Polygon indexer supports wrap-around

                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(pA, transform), UV = Vector2.Transform(pA, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(pB, transform), UV = Vector2.Transform(pB, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(pC, transform), UV = Vector2.Transform(pC, _uvTransform) };

                pB = pC;
            }

            _pg.FillTriangles(vertices, _texture);
        }

        public Matrix3x2 CalculateTextureProjection(in ITexture texture, in Vector2 textureOrigin, in float texelsPerUnit)
        {
            return Matrix3x2.CreateTranslation(-textureOrigin) * Matrix3x2.CreateScale(texelsPerUnit / texture.Size);
        }

        public void Dispose()
        {
            _textTriangleBuffer.Dispose();
        }

        public void SetViewportSize(Vector2I viewportSize)
        {
            _viewportSize = viewportSize;
        }
    }
}
