using System.Drawing;
using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Game.Graphics
{
    public interface IGraphics
    {
        void ClearScreen(in Color color);
        void FillPolygon(in Polygon8 polygon, in Color color);
        void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color);
    }
}
