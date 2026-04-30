using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Geometry.Triangulation;
using BareBone.Geometry.Triangulation;
using Barebone.Graphics;
using Barebone.Graphics.Text;
using Barebone.Messaging;

namespace Barebone.Game.Graphics
{
    internal class GraphicsSubSystem : IGraphics, IDisposable
    {
        // TODO Optimizations:
        // * Combine draw calls with same texture and transforms into a single call
        // * Support Indexed drawing so we can skip the copy work in FillTriangles and send IndexTriangle[] directly (or copy it in bulk).
        // * More possibilities are to be found probably, used AI for the bloom code and it made this a bit messy

        private readonly IPlatformGraphics _pg;
        private readonly IMessageBus _messageBus;
        private ITexture? _texture;
        private Matrix3x2 _uvTransform;
        private Matrix3x2 _worldTransform = Matrix3x2.Identity;
        private float _z;
        private readonly Font _defaultFont;
        private readonly BBList<Vertex> _textTriangleBuffer = new();
        private ICamera _activeCamera;
        public Vector2I ViewportSize { get; private set; }

        public GraphicsSubSystem(IPlatformGraphics pg, IMessageBus messageBus, float windowHeight)
        {
            _pg = pg;
            _messageBus = messageBus;
            _defaultFont = Font.FromBMFontFile("UbuntuMono32.fnt", GetTexture("UbuntuMono32_0.png"));
            _activeCamera = new Camera(messageBus)
            {
                LogicalViewHeight = windowHeight,
                ScreenOrigin = ScreenOrigin.Center,
                Zoom = 1,
                Position = Vector2.Zero
            };
            SetBloom(BloomConfig.None);
        }

        public void ClearScreen(in Color color)
        {
            _pg.ClearScreen(color);
        }

        public void FillPolygon(in Polygon8 polygon, in Color? color = null)
        {
            Span<IndexTriangle> triangles = stackalloc IndexTriangle[Triangulator.GetTriangleCount(polygon.Count)];
            Triangulator.Triangulate(polygon.AsReadOnlySpan(), triangles);
            
            var corners = polygon.AsReadOnlySpan();

            FillTriangles(corners, triangles, color);
        }

        public void FillPolygon(in ReadOnlySpan<Vector2> polygon, in Color? color = null)
        {
            Span<IndexTriangle> triangles = stackalloc IndexTriangle[Triangulator.GetTriangleCount(polygon.Length)];
            Triangulator.Triangulate(polygon, triangles);
            FillTriangles(polygon, triangles, color);
        }

        /// <summary>
        /// Fast alternative to FillPolygon if you guarantee the polygon is convex.
        /// </summary>
        public void FillPolygonConvex(in Polygon8 polygon, in Color? color = null)
        {
            Span<IndexTriangle> triangles = stackalloc IndexTriangle[TriangulatorConvex.GetTriangleCount(polygon.Count)];
            TriangulatorConvex.Triangulate(polygon.Count, triangles);

            var corners = polygon.AsReadOnlySpan();

            FillTriangles(corners, triangles, color);
        }

        public void FillTriangles(ReadOnlySpan<Vector2> corners, Span<IndexTriangle> indexTriangles, Color? color)
        {
            var colorF = ColorF.FromColor(color ?? Color.White);
            Span<Vertex> vertices = stackalloc Vertex[indexTriangles.Length * 3];

            var i = 0;

            var modelToUv = _worldTransform * _uvTransform;

            foreach (var triangle in indexTriangles)
            {
                var a = corners[triangle.A];
                var b = corners[triangle.B];
                var c = corners[triangle.C];

                vertices[i++] = new() { Color = colorF, Position = a, UV = Vector2.Transform(a, modelToUv) };
                vertices[i++] = new() { Color = colorF, Position = b, UV = Vector2.Transform(b, modelToUv) };
                vertices[i++] = new() { Color = colorF, Position = c, UV = Vector2.Transform(c, modelToUv) };
            }

            FillTrianglesInternal(vertices, _texture);
        }

        public void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color)
        {
            FillCircleInternal(center, radius, segmentCount, color);
        }

        public void DrawText(Vector2 position, in string text, in Color color, in float scale = 1f, bool center = false)
        {
            _textTriangleBuffer.Clear();
            var colorF = ColorF.FromColor(color);
            if (center)
            {
                var size = _defaultFont.Measure(text, scale);
                position -= size * 0.5f;
            }
            _defaultFont.AppendString(true, _textTriangleBuffer, text, colorF, position, scale);

            FillTrianglesInternal(_textTriangleBuffer.AsReadOnlySpan(), _defaultFont.Texture);
        }

        public void SetCamera(in ICamera camera)
        {
            _activeCamera = camera;
        }

        public void SetWorldTransform(in Matrix3x2 world, in float z)
        {
            if (z is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(z), "z should lay within [0,1]");
            _worldTransform = world;
            _z = z;
        }

        public void ResetWorldTransform()
        {
            _worldTransform = Matrix3x2.Identity;
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
            return new Camera(_messageBus)
            {
                LogicalViewHeight = viewHeight,
                ScreenOrigin = screenOrigin
            };
        }

        private void FillCircleInternal(in Vector2 center, in float radius, in int segmentCount, in Color color, in float zLayer = 0)
        {
            var colorF = ColorF.FromColor(color);

            Span<Vertex> vertices = stackalloc Vertex[segmentCount * 3];

            var i = 0;

            var angleStep = Angles._360 / segmentCount;

            var angle = -angleStep;
            var a = center + angle.AngleToVector2(radius);

            // UV is computed from world-space position (the world transform is applied first, then uv projection).
            // Position stays in MODEL space; the platform applies world * camera on the GPU.
            var modelToUv = _worldTransform * _uvTransform;

            for (var s = 0; s < segmentCount; s++)
            {
                angle += angleStep;

                var b = center + angle.AngleToVector2(radius);

                vertices[i++] = new() { Color = colorF, Position = center, UV = Vector2.Transform(center, modelToUv) };
                vertices[i++] = new() { Color = colorF, Position = a,      UV = Vector2.Transform(a,      modelToUv) };
                vertices[i++] = new() { Color = colorF, Position = b,      UV = Vector2.Transform(b,      modelToUv) };

                a = b;
            }

            FillTrianglesInternal(vertices, _texture);
        }

        /// <summary>
        /// Sole and central drawing facade into the platform specific graphics system.
        /// </summary>
        private void FillTrianglesInternal(ReadOnlySpan<Vertex> vertices, ITexture? texture)
        {
            _pg.SetTransform(_worldTransform, _activeCamera.WorldToScreenTransform);
            _pg.FillTriangles(vertices, texture, _z);
        }

        public Matrix3x2 CalculateTextureProjection(in ITexture texture, in Vector2 textureOrigin, in float texelsPerUnit)
        {
            return Matrix3x2.CreateTranslation(-textureOrigin) * Matrix3x2.CreateScale(texelsPerUnit / texture.Size);
        }

        public void SetBloom(in BloomConfig config)
        {
            _pg.BloomThreshold = config.BloomThreshold;
            _pg.BloomBrightIntensity = config.BloomBrightIntensity;
            _pg.BloomFinalIntensity = config.BloomFinalIntensity;
            _pg.BloomSoftKnee = config.BloomSoftKnee;
            _pg.BloomUpsampleStrength = config.BloomUpsampleStrength;
        }

        public BloomConfig GetBloom()
        {
            return new(_pg.BloomThreshold, _pg.BloomSoftKnee, _pg.BloomBrightIntensity, _pg.BloomUpsampleStrength, _pg.BloomFinalIntensity);
        }

        public void SetViewportSize(Vector2I viewportSize)
        {
            ViewportSize = viewportSize;
            BB.MessageBus.Publish(new ViewportSizeChanged(viewportSize));
        }

        public ICamera Camera => _activeCamera;
        public Matrix3x2 WorldTransform => _worldTransform;

        public void Dispose()
        {
            _textTriangleBuffer.Dispose();
        }
    }
}
