using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    

    public sealed class AttributeSet : Poolable
    {
        // todo Make a poolable Dictionary
        private readonly Dictionary<string, IAttributeArray> _attributes = new();

        public AttributeArray<T> Add<T>(string name, int capacity) where T : struct
        {
            var attributeArray = Pool.Rent<AttributeArray<T>>();
            attributeArray.Name = name;
            attributeArray.EnsureCapacity(capacity);
            _attributes[name] = attributeArray;
            return attributeArray;
        }

        public AttributeArray<T> Get<T>(string name) where T : struct
        {
            return (AttributeArray<T>)_attributes[name];
        }

        public bool TryGet<T>(string name, out AttributeArray<T>? attribute) where T : struct
        {
            if (_attributes.TryGetValue(name, out var attr))
            {
                attribute = (AttributeArray<T>)attr;
                return true;
            }

            attribute = null;
            return false;
        }

        protected internal override void Construct()
        {
            _attributes.Clear(); // todo make poolable dictionary
        }

        protected internal override void Destruct()
        {
            foreach (var attributeArray in _attributes.Values)
            {
                attributeArray.Return();
            }
            _attributes.Clear();
        }
    }
}
