using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BareBone.Geometry.Triangulation
{
    [StructLayout(LayoutKind.Sequential)]
    public record struct IndexTriangle(int A, int B, int C);

    /// <summary>
    /// Stateless ear-clipping triangulator for convex or concave polygons. No heap allocations.
    /// </summary>
    public static class TriangulatorNoAlloc
    {
        /// <summary>
        /// A simple polygon with N corners always produces exactly N - 2 triangles.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTriangleCount(in int cornerCount) => cornerCount - 2;

        /// <summary>
        /// Triangulates a clockwise polygon. The polygon is allowed to be concave. Fills <paramref name="resultBuffer"/>
        /// with index triangles referring directly into <paramref name="corners"/>. Returns the number of triangles written.
        /// Use <see cref="GetTriangleCount"/> to size <paramref name="resultBuffer"/>.
        /// </summary>
        /// <remarks>
        /// Based on https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf but extended with more features.
        /// </remarks>
        public static int Triangulate(ReadOnlySpan<Vector2> corners, Span<IndexTriangle> resultBuffer)
        {
            if (corners.Length < 3)
                throw new Exception("Can only triangulate polygons with at least 3 points.");

            Span<PointNode> nodes = stackalloc PointNode[corners.Length];
            for (var i = 0; i < corners.Length; i++)
            {
                nodes[i] = new PointNode
                {
                    VertexIndex = i,
                    Previous = (i - 1 + corners.Length) % corners.Length,
                    Next = (i + 1) % corners.Length
                };
            }

            return TriangulateCore(corners, nodes, resultBuffer);
        }

        /// <summary>
        /// Triangulates a clockwise index-based polygon. The polygon is allowed to be concave. Fills <paramref name="resultBuffer"/>
        /// with index triangles whose values are taken from <paramref name="indices"/> (i.e. they refer into <paramref name="corners"/>).
        /// </summary>
        public static int Triangulate(ReadOnlySpan<Vector2> corners, ReadOnlySpan<int> indices, Span<IndexTriangle> resultBuffer)
        {
            if (indices.Length < 3)
                throw new Exception("Can only triangulate polygons with at least 3 points.");

            Span<PointNode> nodes = stackalloc PointNode[indices.Length];
            for (var i = 0; i < indices.Length; i++)
            {
                nodes[i] = new PointNode
                {
                    VertexIndex = indices[i],
                    Previous = (i - 1 + indices.Length) % indices.Length,
                    Next = (i + 1) % indices.Length
                };
            }

            return TriangulateCore(corners, nodes, resultBuffer);
        }

        private static int TriangulateCore(ReadOnlySpan<Vector2> corners, Span<PointNode> nodes, Span<IndexTriangle> resultBuffer)
        {
            Span<int> reflexList = stackalloc int[nodes.Length];
            var reflexCount = 0;
            Span<int> ears = stackalloc int[nodes.Length];
            var earCount = 0;

            // Identify the reflex corners. Everything else is convex (or straight) and a candidate ear.
            for (var i = 0; i < nodes.Length; i++)
            {
                if (IsReflex(corners, nodes, i))
                    reflexList[reflexCount++] = i;
            }

            // Find all initial ears among the convex corners.
            for (var i = 0; i < nodes.Length; i++)
            {
                if (!IsReflex(corners, nodes, i)
                    && IsUnpenetratedEar(corners, nodes, i, reflexList[..reflexCount]))
                {
                    ears[earCount++] = i;
                }
            }

            // Cut off ears one by one, updating the neighbours each time, until the polygon is fully triangulated.
            var triangleCount = 0;
            while (earCount > 0)
            {
                var current = ears[0];
                var prev = nodes[current].Previous;
                var next = nodes[current].Next;

                resultBuffer[triangleCount++] = new IndexTriangle(
                    nodes[prev].VertexIndex,
                    nodes[current].VertexIndex,
                    nodes[next].VertexIndex);

                if (nodes[next].Next == prev)
                    break; // this was the last triangle

                // Detach the ear vertex from the linked list.
                nodes[prev].Next = next;
                nodes[next].Previous = prev;
                RemoveAt(ears, ref earCount, 0);

                // Reevaluate reflex/convex/ear properties of the neighbours, which have changed now the ear is cut off.
                ReevaluateNeighbour(corners, nodes, prev, reflexList, ref reflexCount, ears, ref earCount);
                ReevaluateNeighbour(corners, nodes, next, reflexList, ref reflexCount, ears, ref earCount);
            }

            return triangleCount;
        }

        private static void ReevaluateNeighbour(ReadOnlySpan<Vector2> corners, Span<PointNode> nodes, int neighbourIdx,
            Span<int> reflexList, ref int reflexCount, Span<int> ears, ref int earCount)
        {
            if (IsReflex(corners, nodes, neighbourIdx))
                return;

            // If the neighbour was reflex, promote it to convex by removing it from the reflex list.
            var reflexIdx = IndexOf(reflexList[..reflexCount], neighbourIdx);
            if (reflexIdx >= 0)
                RemoveAt(reflexList, ref reflexCount, reflexIdx);

            if (IsUnpenetratedEar(corners, nodes, neighbourIdx, reflexList[..reflexCount]))
            {
                if (IndexOf(ears[..earCount], neighbourIdx) < 0)
                    ears[earCount++] = neighbourIdx; // promote to ear
            }
            else
            {
                var earIdx = IndexOf(ears[..earCount], neighbourIdx);
                if (earIdx >= 0)
                    RemoveAt(ears, ref earCount, earIdx); // demote from ear
            }
        }

        private static bool IsReflex(ReadOnlySpan<Vector2> corners, ReadOnlySpan<PointNode> nodes, int nodeIdx)
        {
            var node = nodes[nodeIdx];
            return Corner2.GetCornerType(
                corners[nodes[node.Previous].VertexIndex],
                corners[node.VertexIndex],
                corners[nodes[node.Next].VertexIndex]) == CornerType.Reflex;
        }

        /// <summary>
        /// Checks if the ear-triangle at <paramref name="pointIdx"/> is penetrated by any reflex corner of the polygon.
        /// </summary>
        private static bool IsUnpenetratedEar(ReadOnlySpan<Vector2> corners, ReadOnlySpan<PointNode> nodes, int pointIdx, ReadOnlySpan<int> reflexList)
        {
            var point = nodes[pointIdx];
            for (var i = 0; i < reflexList.Length; i++)
            {
                var reflexNodeIdx = reflexList[i];
                if (reflexNodeIdx == point.Next || reflexNodeIdx == point.Previous)
                    continue; // neighbour corners don't count for ear testing.
                if (Triangle2.Contains(
                        corners[nodes[point.Previous].VertexIndex],
                        corners[point.VertexIndex],
                        corners[nodes[point.Next].VertexIndex],
                        corners[nodes[reflexNodeIdx].VertexIndex]))
                    return false;
            }

            return true;
        }

        private static int IndexOf(ReadOnlySpan<int> span, int value)
        {
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == value)
                    return i;
            }
            return -1;
        }

        private static void RemoveAt(Span<int> span, ref int count, int index)
        {
            for (var i = index; i < count - 1; i++)
                span[i] = span[i + 1];
            count--;
        }

        /// <summary>
        /// A node in the doubly linked list of polygon corners. Uses indices into the working buffer instead of references,
        /// so the entire list can live in a single <c>stackalloc</c>'d span.
        /// </summary>
        private struct PointNode
        {
            public int VertexIndex;
            public int Next;
            public int Previous;
        }
    }
}
