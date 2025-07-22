namespace Barebone.Geometry.Triangulation
{
    internal class PointNode
    {
        public readonly int Index;
        public readonly int VertexIndex;
        public PointNode? Next, Previous;

        public PointNode(int index, int vertexIndex)
        {
            Index = index;
            VertexIndex = vertexIndex;
        }

        public override string ToString()
        {
            return $"{Index}";
        }
    }
}