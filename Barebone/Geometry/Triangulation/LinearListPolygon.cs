using Barebone.Geometry.Triangulation;

namespace BareBone.Geometry.Triangulation
{
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

        public IndexPolygon ToPolygon()
        {
            var indices = new int[Count];
            var current = PointListRoot;
            for (var i = 0; i < Count; i++)
            {
                indices[i] = current!.Index;
                current = current.Next;
            }
            return new IndexPolygon(indices);
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
}
