using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public interface IAttributeArray : IPoolable 
    {
       public string Name { get; }

       /// <summary>
       /// Returns a newly rented deep clone.
       /// </summary>
       IAttributeArray Clone();
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
    }
}
