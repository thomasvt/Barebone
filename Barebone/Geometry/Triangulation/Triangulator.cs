using System.Numerics;

namespace BareBone.Geometry.Triangulation
{
    // this is the old triangulator. It works, but allocates many small PointNodes on heap each use. 
    // Tried to make TriangulatorNoAlloc but it's buggy.

    internal class PointNode(int index, int vertexIndex)
    {
        public readonly int Index = index;
        public readonly int VertexIndex = vertexIndex;
        public PointNode? Next, Previous;

        public override string ToString()
        {
            return $"{Index}";
        }
    }

    internal class LinearListPolygon
    {
        public readonly PointNode PointListRoot;

        public LinearListPolygon(PointNode pointListRoot)
        {
            PointListRoot = pointListRoot;
            var count = 1;
            pointListRoot = PointListRoot.Next!;
            while (pointListRoot != PointListRoot)
            {
                count++;
                pointListRoot = pointListRoot.Next!;
            }

            Count = count;
        }

        /// <summary>
        /// Builds a linearlist polygon for a polygon where each index matches 1 on 1 with a corner and no intermediate index array is therefore needed.
        /// </summary>
        public static LinearListPolygon From1To1Polygon(int cornerCount)
        {
            var root = new PointNode(0, 0);
            var current = root;
            for (var i = 1; i < cornerCount; i++)
            {
                current.Next = new PointNode(i, i)
                {
                    Previous = current
                };
                current = current.Next;
            }

            current.Next = root;
            root.Previous = current;

            return new LinearListPolygon(root);
        }

        /// <summary>
        /// Builds a linearlist polygon from the cornerindices of a polygon.
        /// </summary>
        public static LinearListPolygon FromIndexPolygon(ReadOnlySpan<int> indices)
        {
            var root = new PointNode(0, indices[0]);
            var current = root;
            for (var i = 1; i < indices.Length; i++)
            {
                current.Next = new PointNode(i, indices[i])
                {
                    Previous = current
                };
                current = current.Next;
            }

            current.Next = root;
            root.Previous = current;

            return new LinearListPolygon(root);
        }

        public int Count { get; }
    }

    /// <summary>
    /// Triangulator for convex and concave triangles. 
    /// </summary>
    public class Triangulator
    {
        private readonly List<PointNode> _inflexList = new();
        private readonly List<PointNode> _reflexList = new();

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
        public void Triangulate(ReadOnlySpan<Vector2> corners, Span<IndexTriangle> triangleBuffer)
        {
            Triangulate(corners, LinearListPolygon.From1To1Polygon(corners.Length), triangleBuffer);
        }

        /// <summary>
        /// Trianglulates a clockwise index-based polygon. The polygon is allowed to be concave. Fills your resultIndexBuffer with a triangle-list of indices taken from your 'indices'.
        /// </summary>
        public void Triangulate(ReadOnlySpan<Vector2> corners, ReadOnlySpan<int> indices, Span<IndexTriangle> triangleBuffer)
        {
            Triangulate(corners, LinearListPolygon.FromIndexPolygon(indices), triangleBuffer);
        }

        public static int GetTriangleCount(int cornerCount)
        {
            return cornerCount - 2;
        }

        /// <summary>
        /// Trianglulates a clockwise polygon. The polygon is allowed to be concave.
        /// </summary>
        /// <remarks>
        /// Based on https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf but extended with more features.
        /// </remarks>
        private void Triangulate(ReadOnlySpan<Vector2> corners, LinearListPolygon linearPolygon, Span<IndexTriangle> triangleBuffer)
        {
            if (corners.Length < 3)
                throw new Exception($"Can only triangulate polygons with at least 3 points.");

            var idx = 0;

            _inflexList.EnsureCapacity(corners.Length);
            _reflexList.EnsureCapacity(corners.Length);
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

                triangleBuffer[idx++] = new IndexTriangle(current.Previous!.VertexIndex, current.VertexIndex, current.Next!.VertexIndex);

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
