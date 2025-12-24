using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public sealed class Geometry : Poolable
    {
        public PointSet Points { get; private set; }
        public SegmentSet Segments { get; private set; }
        public PathSet Paths { get; private set; }
        public ShapeSet Shapes { get; private set; }
        public GroupSet Groups { get; private set; }
        public AttributeSet GlobalAttributes { get; private set; }

        protected internal override void Construct()
        {
            Points = Pool.Rent<PointSet>();
            Segments = Pool.Rent<SegmentSet>();
            Paths = Pool.Rent<PathSet>();
            Shapes = Pool.Rent<ShapeSet>();
            Groups = Pool.Rent<GroupSet>();
            GlobalAttributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            Points.Return();
            Segments.Return();
            Paths.Return();
            Shapes.Return();
            Groups.Return();
            GlobalAttributes.Return();
        }

        public static Geometry RentNew()
        {
            return Pool.Rent<Geometry>();
        }

        public void CopyTo(in Geometry destination)
        {
            throw new NotImplementedException();
        }
    }
}
