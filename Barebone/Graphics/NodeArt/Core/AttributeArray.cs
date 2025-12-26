using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public interface IAttributeArray : IPoolable 
    {
       public string Name { get; }

       /// <summary>
       /// Returns a newly rented deep clone.
       /// </summary>
       IAttributeArray Clone();

       void SetSize(int size);
       void Clear();
    }

    public sealed class AttributeArray<T> : Poolable, IAttributeArray where T : struct
    {
        public string Name { get; internal set; } = null!;
        private BBList<T> Values { get; set; } = null!;

        public IAttributeArray Clone()
        {
            var clone = Pool.Rent<AttributeArray<T>>();
            clone.Name = Name;
            clone.EnsureCapacity(Values.Count);
            clone.Values.AddBBList(Values);
            return clone;
        }

        public void Clear()
        {
            Values.Clear();
        }

        protected internal override void Construct()
        {
            Values = Pool.Rent<BBList<T>>();
        }

        protected internal override void Destruct()
        {
            Values.Return();
        }

        public void EnsureCapacity(int capacity)
        {
            Values.EnsureCapacity(capacity);
        }

        public Span<T> AsSpan()
        {
            return Values.AsSpan();
        }

        public T Get(int idx)
        {
            if (idx < 0 || idx >= Values.Count)
                throw new IndexOutOfRangeException($"Index {idx} is out of range [0..{Values.Count})");

            return Values.InternalArray[idx];
        }

        public void Set(int idx, T value)
        {
            if (idx < 0 || idx >= Values.Count)
                throw new IndexOutOfRangeException($"Index {idx} is out of range [0..{Values.Count})");

            Values.InternalArray[idx] = value;
        }

        public void SetSize(int size)
        {
            Values.SetFixedCount(size, true);
        }

        public int Size => Values.Count;
    }
}
