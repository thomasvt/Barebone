using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public sealed class Path : Poolable
    {
        public BBList<int> SegmentIndices { get; private set; } = null!;
        protected internal override void Construct()
        {
            SegmentIndices = Pool.Rent<BBList<int>>();
        }

        protected internal override void Destruct()
        {
            SegmentIndices.Return();
        }
    }
}
