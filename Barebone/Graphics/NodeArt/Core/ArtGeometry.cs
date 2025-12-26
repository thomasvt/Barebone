using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public sealed class ArtGeometry : Poolable
    {
        public PointSet PointSet { get; private set; } = null!;
        public SegmentSet SegmentSet { get; private set; } = null!;
        public PathSet PathSet { get; private set; } = null!;
        public ShapeSet ShapeSet { get; private set; } = null!;
        public AttributeSet GlobalAttributes { get; private set; } = null!;

        protected internal override void Construct()
        {
            PointSet = Pool.Rent<PointSet>();
            SegmentSet = Pool.Rent<SegmentSet>();
            PathSet = Pool.Rent<PathSet>();
            ShapeSet = Pool.Rent<ShapeSet>();
            GlobalAttributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            PointSet.Return();
            SegmentSet.Return();
            PathSet.Return();
            ShapeSet.Return();
            GlobalAttributes.Return();
        }

        public static ArtGeometry RentNew()
        {
            return Pool.Rent<ArtGeometry>();
        }

        /// <summary>
        /// Makes destination a deep clone of this geometry.
        /// </summary>
        public void CloneTo(in ArtGeometry destination)
        {
            PointSet.CloneTo(destination.PointSet);
            SegmentSet.CloneTo(destination.SegmentSet);
            PathSet.CloneTo(destination.PathSet);
            ShapeSet.CloneTo(destination.ShapeSet);
            GlobalAttributes.CloneTo(destination.GlobalAttributes);
        }

        public void Clear()
        {
            PointSet.Clear();
            SegmentSet.Clear();
            PathSet.Clear();
            ShapeSet.Clear();
            GlobalAttributes.Clear();
        }
    }
}
