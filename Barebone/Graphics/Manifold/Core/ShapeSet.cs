using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public sealed class ShapeSet : Poolable
    {
        public BBList<Shape> Shapes { get; private set; } = null!;

        public AttributeSet Attributes { get; private set; } = null!;

        protected internal override void Construct()
        {
            Shapes = Pool.Rent<BBList<Shape>>();
            Attributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            Shapes.Return();
            Attributes.Return();
        }
    }
}
