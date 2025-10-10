using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Spatial
{
    public class HeatMap(AabbI gridRange)
    {
        private readonly byte[] _cells = new byte[gridRange.Size.X * gridRange.Size.Y];

        public void Mark(AabbI aabb, byte value)
        {
            aabb = gridRange.GetIntersection(aabb);

            var offsetX = aabb.MinCorner.X - gridRange.MinCorner.X;
            for (var y = aabb.MinCorner.Y; y < aabb.MaxCornerExcl.Y; y++)
            {
                var offsetY = y - gridRange.MinCorner.Y;
                var offset = offsetY * gridRange.Width + offsetX;
                Array.Fill(_cells, value, offset, aabb.Width);
            }
        }

        public AabbI GetCellRange(Aabb worldRange)
        {
            var minCorner = worldRange.MinCorner.Floor();
            var maxCornerExcl = worldRange.MaxCorner.Floor() + Vector2I.One;
            return new AabbI(minCorner, maxCornerExcl);
        }

        public byte Get(Vector2I cell)
        {
            var offsetX = cell.X - gridRange.MinCorner.X;
            var offsetY = cell.Y - gridRange.MinCorner.Y;
            var offset = offsetX + offsetY * gridRange.Width;
            return _cells[offset];
        }

        public Vector2I GetCell(Vector2 worldCoords)
        {
            return worldCoords.Floor();
        }
    }
}
