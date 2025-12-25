using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public sealed class PathSet : Poolable
    {
        public BBList<Path> Paths { get; private set; } = null!;

        public AttributeSet Attributes { get; private set; } = null!;
        protected internal override void Construct()
        {
            Paths = Pool.Rent<BBList<Path>>();
            Attributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            Paths.Return();
            Attributes.Return();
        }

        public void Clear()
        {
            Paths.Clear();
            Attributes.Clear();
        }

        public void CloneTo(PathSet dest)
        {
            dest.Paths.Clear();
            dest.Paths.AddBBList(Paths);;
            Attributes.CloneTo(dest.Attributes);
        }

        public int AddPath(int firstSegmentIdx, int lastSegmentIdx)
        {
            var idx = Paths.Count;
            Paths.Add(new Path(idx, firstSegmentIdx, lastSegmentIdx));
            return idx;
        }
    }
}
