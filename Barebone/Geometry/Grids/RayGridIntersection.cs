using System.Numerics;

namespace Barebone.Geometry.Grids
{
    public static class RayGridIntersection
    {
        /// <summary>
        /// Enumerates all cells of an infinite 2D grid with cells of size 1 that are touched by a float-based ray, in the correct order.
        /// </summary>
        public static IEnumerable<Vector2I> VisitRayAmantidesWoo(Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var tMax = ab.Length();
            var direction = ab / tMax;

            var x = (int)MathF.Floor(a.X);
            int dirX;
            float tStepX;
            float tNextX;
            if (direction.X > 0.0)
            {
                dirX = 1;
                tStepX = 1f / direction.X;
                tNextX = GetNextCellBound(a.X, direction.X);

            }
            else if (direction.X < 0.0)
            {
                dirX = -1;
                tStepX = 1f / -direction.X;
                tNextX = GetNextCellBound(a.X, direction.X);
            }
            else
            {
                dirX = 0;
                tStepX = float.MaxValue;
                tNextX = float.MaxValue;
            }

            var y = (int)MathF.Floor(a.Y);
            int dirY;
            float tStepY;
            float tNextY;
            if (direction.Y > 0.0)
            {
                dirY = 1;
                tStepY = 1f / direction.Y;
                tNextY = GetNextCellBound(a.Y, direction.Y);

            }
            else if (direction.Y < 0.0)
            {
                dirY = -1;
                tStepY = 1f / -direction.Y;
                tNextY = GetNextCellBound(a.Y, direction.Y);
            }
            else
            {
                dirY = 0;
                tStepY = float.MaxValue;
                tNextY = float.MaxValue;
            }

            yield return new Vector2I(x, y);

            while (tNextX < tMax || tNextY < tMax)
            {
                if (tNextX < tNextY)
                {
                    // X-axis traversal.
                    x += dirX;
                    tNextX += tStepX;
                }
                else
                {
                    // Y-axis traversal.
                    y += dirY;
                    tNextY += tStepY;
                }

                yield return new Vector2I(x, y);
            }
        }

        private static float GetNextCellBound(float from, float delta)
        {
            return delta switch
            {
                > 0 => (float)(Math.Ceiling(from) - from) / Math.Abs(delta),
                < 0 => (float)(from - Math.Floor(from)) / Math.Abs(delta),
                _ => float.MaxValue
            };
        }
    }
}
