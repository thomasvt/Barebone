using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;

namespace Barebone.Game.Graphics
{
    internal class GraphicsSubSystem(IPlatformGraphics pg, Camera camera) : IGraphics
    {
        private ITexture? _texture;
        private Matrix3x2 _uvTransform;

        public void ClearScreen(in Color color)
        {
            pg.ClearScreen(color);
        }

        public void FillPolygon(in Polygon8 polygon, in Color? color = null)
        {
            FillPolygonInternal(polygon, color ?? Color.White);
        }

        public void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color)
        {
            FillCircleInternal(center, radius, segmentCount, color);
        }

        public void UnsetTexture()
        {
            _texture = null;
            _uvTransform = Matrix3x2.Identity;
        }

        public void SetTexture(in ITexture texture, in Matrix3x2 uvTransform)
        {
            _texture = texture;
            _uvTransform = uvTransform;
        }

        public ITexture GetTexture(string assetPath)
        {
            return pg.GetTexture(assetPath);
        }

        private void FillCircleInternal(in Vector2 center, in float radius, in int segmentCount, in Color color)
        {
            var colorF = ColorF.FromColor(color);

            Span<Vertex> vertices = stackalloc Vertex[segmentCount * 3];

            var i = 0;

            var angleStep = Angles._360 / segmentCount;

            var angle = -angleStep;
            var a = center + angle.AngleToVector2(radius);

            for (var s = 0; s < segmentCount; s++)
            {
                angle += angleStep;

                var b = center + angle.AngleToVector2(radius);

                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(center, camera.WorldToScreenTransform), UV = Vector2.Transform(center, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(a, camera.WorldToScreenTransform), UV = Vector2.Transform(a, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(b, camera.WorldToScreenTransform), UV = Vector2.Transform(b, _uvTransform) };

                a = b;
            }

            pg.FillTriangles(vertices, _texture);
        }

        private void FillPolygonInternal(in Polygon8 polygon, in Color color)
        {
            var colorF = ColorF.FromColor(color);

            var pA = polygon[0];
            Span<Vertex> vertices = stackalloc Vertex[(polygon.Count - 2) * 3];

            var i = 0;
            var pB = polygon[1];

            for (var c = 2; c < polygon.Count; c++)
            {
                var pC = polygon[c]; // Polygon indexer supports wrap-around

                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(pA, camera.WorldToScreenTransform), UV = Vector2.Transform(pA, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(pB, camera.WorldToScreenTransform), UV = Vector2.Transform(pB, _uvTransform) };
                vertices[i++] = new() { Color = colorF, Position = Vector2.Transform(pC, camera.WorldToScreenTransform), UV = Vector2.Transform(pC, _uvTransform) };

                pB = pC;
            }

            pg.FillTriangles(vertices, _texture);
        }
    }
}
