using System.Drawing;
using Barebone.Geometry;

namespace Barebone.Game.Graphics
{
    public interface IDraw
    {
        void ClearScreen(in Color color);
        void FillAabb(in Aabb box, in Color color);
        void FillPolygon(in Polygon8 polygon, in Color color);
    }
}
