namespace Barebone.Graphics.Manifold.Core
{
    public readonly ref struct WrangleContext
    {
        public readonly Geometry Geometry;
        public readonly int Index;
        public readonly GeometryDomain Domain;
        public readonly float Time;
    }
}
