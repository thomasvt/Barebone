using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using BareBone.Geometry.Triangulation;
using Barebone.Graphics;

namespace Barebone.Game.Graphics
{
    public interface IGraphics
    {
        Vector2I ViewportSize { get; }
        ICamera Camera { get; }
        Matrix3x2 WorldTransform { get; }
        void ClearScreen(in Color color);
        void FillPolygon(in Polygon8 polygon, in Color? color = null);
        void FillPolygon(in ReadOnlySpan<Vector2> polygon, in Color? color = null);

        /// <summary>
        /// Fast alternative to FillPolygon if you guarantee the polygon is convex.
        /// </summary>
        void FillPolygonConvex(in Polygon8 polygon, in Color? color = null);

        void LinePolygon(in Polygon8 polygon, in float lineWidth, in Color? color = null);
        void FillTriangles(ReadOnlySpan<Vector2> corners, Span<IndexTriangle> indexTriangles, Color? color);
        void Line(in Vector2 a, in Vector2 b, in float lineWidth, in Color? color = null, in LineCap lineCap = LineCap.Butt);
        void FillRectangle(in Aabb aabb, Color? color = null);
        void LineRectangle(in Aabb aabb, float lineWidth, Color? color = null);
        void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color);
        void DrawText(Vector2 position, in string text, in Color color, in float scale = 1f, bool center = false);
        void SetCamera(in ICamera camera);
        void SetWorldTransform(in Matrix3x2 world, in float z);
        void ResetWorldTransform();
        void SetColorOnly();
        void SetTexture(in ITexture texture, in Matrix3x2 projection);
        ITexture GetTexture(string assetPath);
        ICamera CreateCamera(float viewHeight, ScreenOrigin screenOrigin);
        Matrix3x2 CalculateTextureProjection(in ITexture texture, in Vector2 textureOrigin, in float texelsPerUnit);
        void SetBloomSettings(in BloomSettings settings);
        BloomSettings GetBloomSettings();
        void SetViewportSize(Vector2I viewportSize);
        void Dispose();
    }
}
