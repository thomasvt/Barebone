using System.Collections;

namespace BareBone.Random
{
    /// <summary>
    /// A set of items that have different chances to get picked. THe chance of each item is represented by a whole number `Chance`.
    /// The actual chance of an item being picked is its Chance divided by the total Chance of the ChanceSet.
    /// </summary>
    public class ChanceSet<T> : IEnumerable<ChanceSet<T>.ChanceItem>
    {
        private List<ChanceItem> _items { get; }
        private bool _isDirty = true;
        private List<ChanceItem> PrecalculatedItems
        {
            get
            {
                if (_isDirty)
                {
                    Precalculate();
                    _isDirty = false;
                }
                return _items;
            }
        }

        private void Precalculate()
        {
            var totalWeight = (float)_items.Sum(i => i.Chance);

            var cumul = 0f;
            foreach (var item in _items)
            {
                var chance = item.Chance / totalWeight;
                cumul += chance;
                item.CumulativeChance = cumul;
            }

            _items[^1].CumulativeChance = 1f; // prevent rounding errors
        }

        public ChanceSet()
        {
            _items = new List<ChanceItem>();
            _isDirty = true;
        }

        public ChanceSet(params ChanceItem[] items)
        {
            _items = items.ToList();
            _isDirty = true;
        }

        public ChanceSet(IEnumerable<ChanceItem> items)
        {
            _items = items.ToList();
            _isDirty = true;
        }

        public void Add(T item, int chance)
        {
            _items.Add(new ChanceItem(chance, item));
            _isDirty = true;
        }

        public IEnumerator<ChanceItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public record ChanceItem(int Chance, T Value)
        {
            internal float CumulativeChance;
        }

        public T Pick(float t)
        {
            foreach (var item in PrecalculatedItems)
            {
                if (item.CumulativeChance > t) return item.Value;
            }

            return PrecalculatedItems[^1].Value;
        }

        public void Clear()
        {
            _items.Clear();
            _isDirty = true;
        }
    }
}
