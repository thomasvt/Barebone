using System.Numerics;
using Barebone.Geometry.Triangulation;

namespace BareBone.Geometry.Triangulation
{
    /// <summary>
    /// Triangulator with only reallocations for growing.
    /// </summary>
    public class Triangulator
    {
        public record struct IndexTriangle(int A, int B, int C);

        private readonly List<PointNode> _inflexList = new ();
        private readonly List<PointNode> _reflexList = new ();

        /// <summary>
        /// Single instance for reusing the internally allocated buffers in many triangulations. Not thread safe.
        /// </summary>
        public static Triangulator Shared = new();

        /// <summary>
        /// Trianglulates a clockwise polygon. The polygon is allowed to be concave. Fills your resultIndexBuffer with a triangle-list of indices into your corners list.
        /// </summary>
        /// <remarks>
        /// Based on https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf but extended with more features.
        /// </remarks>
        public IEnumerable<IndexTriangle> Triangulate(ArraySegment<Vector2> corners)
        {
            return Triangulate(corners, LinearListPolygon.From1To1Polygon(corners.Count));
        }

        /// <summary>
        /// Trianglulates a clockwise index-based polygon. The polygon is allowed to be concave. Fills your resultIndexBuffer with a triangle-list of indices taken from your 'indices'.
        /// </summary>
        public IEnumerable<IndexTriangle> Triangulate(ArraySegment<Vector2> corners, ReadOnlySpan<int> indices)
        {
            return Triangulate(corners, LinearListPolygon.FromIndexPolygon(indices));
        }

        public int GetTriangleCount(Vector2[] corners)
        {
            return corners.Length - 2;
        }

        /// <summary>
        /// Trianglulates a clockwise polygon. The polygon is allowed to be concave. Returns a list of indices forming triangle triplets.
        /// </summary>
        /// <remarks>
        /// Based on https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf but extended with more features.
        /// </remarks>
        private IEnumerable<IndexTriangle> Triangulate(ArraySegment<Vector2> corners, LinearListPolygon linearPolygon)
        {
            if (corners.Count < 3)
                throw new Exception($"Can only triangulate polygons with at least 3 points.");

            _inflexList.EnsureCapacity(corners.Count);
            _reflexList.EnsureCapacity(corners.Count);
            PopulateConvexAndReflexLists(corners, linearPolygon, _inflexList, _reflexList);

            // find all initial ears
            var ears = new List<PointNode>(_inflexList.Count);
            foreach (var inflexPoint in _inflexList)
            {
                if (IsUnpenetratedEar(corners, inflexPoint, _reflexList))
                    ears.Add(inflexPoint);
            }

            // cut off all ears and update their neighbour corners to check for new ears while more than 3 corners are left
            while (ears.Any())
            {
                var current = ears[0];

                yield return new IndexTriangle(current.Previous!.VertexIndex, current.VertexIndex, current.Next!.VertexIndex);

                if (current.Next.Next!.Next == current)
                    break; // this was the last triangle

                // delete the ear vertex:
                current.Previous.Next = current.Next;
                current.Next.Previous = current.Previous;
                _inflexList.Remove(current);
                ears.RemoveAt(0);

                // reevaluate reflex/convex properties of the neighbours, which have changed now the ear is cut off.
                ReevaluateNeighbour(corners, current.Previous, _reflexList, _inflexList, ears);
                ReevaluateNeighbour(corners, current.Next, _reflexList, _inflexList, ears);
            }
        }

        private static void ReevaluateNeighbour(ReadOnlySpan<Vector2> vertices, PointNode neighbour, List<PointNode> reflexList, List<PointNode> convexList, List<PointNode> ears)
        {
            if (Corner2.GetCornerType(vertices[neighbour.Previous!.VertexIndex], vertices[neighbour.VertexIndex],
                    vertices[neighbour.Next!.VertexIndex]) == CornerType.Reflex) 
                return;

            if (reflexList.Contains(neighbour))
            {
                // promote to convex list
                reflexList.Remove(neighbour);
                convexList.Add(neighbour);
            }

            if (IsUnpenetratedEar(vertices, neighbour, reflexList))
            {
                if (!ears.Contains(neighbour))
                {
                    // promote to ear
                    ears.Add(neighbour);
                }
            }
            else
            {
                // demote to or leave it as non-ear
                ears.Remove(neighbour); // ... if exists
            }
        }

        /// <summary>
        /// Checks if this ear triangle is penetrated by one of the reflex corner of the polygon.
        /// </summary>
        private static bool IsUnpenetratedEar(ReadOnlySpan<Vector2> vertices, PointNode point, List<PointNode> reflexVertices)
        {
            foreach (var reflexVertex in reflexVertices)
            {
                if (reflexVertex == point.Next || reflexVertex == point.Previous)
                    continue; // neighbour corners don't count for Ear testing.
                if (Triangle2.Contains(vertices[point.Previous!.VertexIndex], vertices[point.VertexIndex], vertices[point.Next!.VertexIndex], vertices[reflexVertex.VertexIndex]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// removes points that have an angle of 0 or 180 degrees, and separates the remaining points in a convex and reflex list.
        /// </summary>
        private static void PopulateConvexAndReflexLists(ReadOnlySpan<Vector2> vertices, LinearListPolygon polygon, List<PointNode> inflexList, List<PointNode> reflexList)
        {
            inflexList.Clear();
            reflexList.Clear();

            var current = polygon.PointListRoot;
            while (true)
            {
                if (Corner2.GetCornerType(vertices[current.Previous!.VertexIndex], vertices[current.VertexIndex], vertices[current.Next!.VertexIndex]) == CornerType.Reflex)
                    reflexList.Add(current);
                else
                    inflexList.Add(current);

                if (current.Next == polygon.PointListRoot)
                    break;
                current = current.Next;
            }
        }

    }
}
