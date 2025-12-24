using System.Collections;
using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public sealed class GroupSet : Poolable
    {
        private readonly Dictionary<string, Group> _groups = new();

        public Group GetOrCreate(string name, GeometryDomain domain, int size)
        {
            if (!_groups.TryGetValue(name, out var group))
            {
                group = Pool.Rent<Group>();
                group.Name = name;
                group.Domain = domain;
                group.Membership = new BitArray(size);
                _groups[name] = group;
            }
            return group;
        }

        public bool TryGet(string name, out Group? group)
            => _groups.TryGetValue(name, out group);

        protected internal override void Construct()
        {
            _groups.Clear();
        }

        protected internal override void Destruct()
        {
            foreach (var group in _groups.Values)
            {
                group.Return();
            }
            _groups.Clear();
        }
    }
}
