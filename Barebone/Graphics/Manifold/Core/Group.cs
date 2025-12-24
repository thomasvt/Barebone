using System.Collections;
using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public sealed class Group : Poolable
    {
        public string Name { get; internal set; } = null!;
        public GeometryDomain Domain { get; internal set; }

        public BitArray Membership { get; internal set; } = null!;

        protected internal override void Construct()
        {
            Name = null!;
            Membership = null!;
        }

        protected internal override void Destruct()
        {
        }
    }
}
