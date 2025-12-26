using Barebone.Pools;
using System.Diagnostics.Contracts;
using System.Xml.Linq;

namespace Barebone.Graphics.NodeArt.Core
{
    

    public sealed class AttributeSet : Poolable
    {

        // todo Make a poolable Dictionary
        private readonly Dictionary<string, IAttributeArray> _attributes = new();

        public AttributeArray<T> Get<T>(string name) where T : struct
        {
            return (AttributeArray<T>)_attributes[name];
        }

        public AttributeArray<T> GetOrCreate<T>(string name, int size) where T : struct
        {
            if (!_attributes.ContainsKey(name))
            {
                var attributeArray = Pool.Rent<AttributeArray<T>>();
                attributeArray.Name = name;
                attributeArray.SetSize(size);
                _attributes[name] = attributeArray;
            }

            return Get<T>(name);
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
            
        }

        protected internal override void Destruct()
        {
            Clear();
        }

        public void Clear()
        {
            foreach (var attributeArray in _attributes.Values)
            {
                attributeArray.Return();
            }
            _attributes.Clear();
        }

        /// <summary>
        /// Makes the dest contain the exact same attributes and data as this one.
        /// </summary>
        public void CloneInto(AttributeSet dest)
        {
            dest.Clear();
            foreach (var arr in _attributes.Values)
            {
                dest._attributes.Add(arr.Name, arr.Clone());
            }
        }

        public void SetItemCount(int count)
        {
            foreach (var arr in _attributes.Values)
            {
                arr.SetSize(count);
            }
        }
    }
}
