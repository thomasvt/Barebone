using System.Drawing;
using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Game.Graphics
{
    public interface IDraw
    {
        void ClearScreen(in Color color);
        void FillAabb(in Aabb box, in Color color);
        void Line(in Vector2 a, in Vector2 b, float width, LineCap lineCap, in Color color);
        void FillPolygon(in Vector2 position, in Polygon8 polygon, in Color color);
        void FillPolygon(in Polygon8 polygon, in Color color);
        void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color);
    }
}
