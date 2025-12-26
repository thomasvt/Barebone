using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public sealed class GeometrySet : Poolable
    {
        public PointSet PointSet { get; private set; } = null!;
        public SegmentSet SegmentSet { get; private set; } = null!;
        public PathSet PathSet { get; private set; } = null!;
        public ShapeSet ShapeSet { get; private set; } = null!;

        protected internal override void Construct()
        {
            PointSet = Pool.Rent<PointSet>();
            SegmentSet = Pool.Rent<SegmentSet>();
            PathSet = Pool.Rent<PathSet>();
            ShapeSet = Pool.Rent<ShapeSet>();
        }

        protected internal override void Destruct()
        {
            PointSet.Return();
            SegmentSet.Return();
            PathSet.Return();
            ShapeSet.Return();
        }

        public static GeometrySet RentNew()
        {
            return Pool.Rent<GeometrySet>();
        }

        /// <summary>
        /// Makes destination a deep clone of this geometry.
        /// </summary>
        public void CloneInto(in GeometrySet destination)
        {
            PointSet.CloneInto(destination.PointSet);
            SegmentSet.CloneInto(destination.SegmentSet);
            PathSet.CloneInto(destination.PathSet);
            ShapeSet.CloneInto(destination.ShapeSet);
        }

        public void SetItemCountsToZero()
        {
            PointSet.SetSize(0);
            SegmentSet.SetSize(0);
            PathSet.SetSize(0);
            ShapeSet.SetSize(0);
        }
    }
}
