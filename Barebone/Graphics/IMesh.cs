using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics.Gpu;
using Barebone.Graphics.Sprites;

namespace Barebone.Graphics
{
    public interface IMesh
    {
        void Clear();

        Mesh FillTriangle(in GpuTexVertex a, in GpuTexVertex b, in GpuTexVertex c);
        Mesh FillTriangle(in Vector3 a, in Vector3 b, in Vector3 c, in Color color);
        Mesh FillTriangle(in Triangle3 t, in Color color);
        Mesh FillTriangleInZ(in Triangle2 t, in float z, in Color color);
        Mesh FillQuad(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, in Color color);
        Mesh FillQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float z, in Color color);
        Mesh FillAabbInZ(in Aabb aabb, in float z, in Color color);
        Mesh StrokeQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float halfWidth, in float z, in Color color);
        Mesh StrokeAabbInZ(in Aabb aabb, in float halfWidth, in float z, in Color color);
        unsafe Mesh FillPolygonConvexInZ(in Polygon8 polygon, in float z, in Color color);
        Mesh StrokePolygonInZ(in Polygon8 polygon, float strokeWidth, in float z, in Color color);
        Mesh PointInZ(in Vector2 position, in float halfSize, in float z, in Color color);
        Mesh FillCircleInZ(in Vector2 center, in float radius, in int segmentCount, in float z, in Color colorIn, in Color? colorOut = null, in float angleOffset = 0f);
        Mesh FillEllipseInZ(in Vector2 center, in Vector2 radius, in int segmentCount, in float z, in Color color);
        Mesh StrokeRegularPolyInZ(in Vector2 center, in float radius, in float strokeWidth, in int segmentCount, in float z, in Color color, in float angleOffset = 0f);
        Mesh LineInZ(in Vector2 a, in Vector2 b, in float halfWidth, in float z, in Color color);
        void DrawSprite(in Vector2 position, in Sprite sprite, in float z, Color? tint = null);
    }
}
