using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
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
    }
}
