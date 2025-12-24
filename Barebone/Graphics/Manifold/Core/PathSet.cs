using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
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
            foreach (var item in Paths.AsReadOnlySpan())
                item.Return();
            Paths.Return();
            Attributes.Return();
        }
    }
}
