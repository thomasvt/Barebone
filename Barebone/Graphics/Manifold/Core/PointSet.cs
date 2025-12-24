using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public sealed class PointSet : Poolable
    {
        public BBList<Point> Points { get; private set; } = null!;

        public AttributeSet Attributes { get; private set; } = null!;

        protected internal override void Construct()
        {
            Points = Pool.Rent<BBList<Point>>();
            Attributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            Points.Return();
            Attributes.Return();
        }
    }

}
