using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public sealed class SegmentSet : Poolable
    {
        public BBList<Segment> Segments { get; private set; } = null!;

        public AttributeSet Attributes { get; private set; } = null!;


        protected internal override void Construct()
        {
            Segments = Pool.Rent<BBList<Segment>>();
            Attributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            Segments.Return();
            Attributes.Return();
        }

        public void Clear()
        {
            Segments.Clear();
            Attributes.Clear();
        }

        public int AddSegment(int aIdx, int bIdx)
        {
            var idx = Segments.Count;
            Segments.Add(new Segment(idx, SegmentType.Line, aIdx, bIdx, -1, -1));
            return idx;
        }

        public void CloneTo(SegmentSet dest)
        {
            dest.Segments.Clear();
            dest.Segments.AddBBList(Segments);
            Attributes.CloneTo(dest.Attributes);
        }
    }
}
