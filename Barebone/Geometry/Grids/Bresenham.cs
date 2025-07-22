namespace Barebone.Geometry.Grids
{
    public static class Bresenham
    {
        /// <summary>
        /// Enumerates all cells on a grid that are touched by a line, in order. Uses Bresenham algorithm. 
        /// </summary>
        /// <param name="noDiagonals">Allows only straight steps along the X or Y axis, never diagonal.</param>
        public static IEnumerable<Vector2I> VisitLineBresenham(Vector2I a, Vector2I b, bool noDiagonals = false)
        {
            var (x0, y0) = a;
            var (x1, y1) = b;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                yield return new Vector2I(x0, y0);

                if (x0 == x1 && y0 == y1) break;

                var e2 = err * 2;

                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                    if (noDiagonals) continue; // step in only 1 dimension at a time.
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// Returns all cells formed by a circle on a grid. The order is not in a radial fashion, but octant-first, radial-second!
        /// </summary>
        public static IEnumerable<Vector2I> VisitCircleBresenham(Vector2I center, int radius)
        {
            var t1 = radius;
            var x = radius;
            var y = 0;

            while (x >= y)
            {
                // visit all octants
                yield return new Vector2I(center.X + x, center.Y + y);
                yield return new Vector2I(center.X - x, center.Y + y);
                yield return new Vector2I(center.X + x, center.Y - y);
                yield return new Vector2I(center.X - x, center.Y - y);
                yield return new Vector2I(center.X + y, center.Y + x);
                yield return new Vector2I(center.X - y, center.Y + x);
                yield return new Vector2I(center.X + y, center.Y - x);
                yield return new Vector2I(center.X - y, center.Y - x);

                y++;

                t1 += y;
                var t2 = t1 - x;
                if (t2 >= 0)
                {
                    t1 = t2;
                    x--;
                }
            }
        }

        
    }
}
