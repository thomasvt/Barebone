using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Barebone;
using Barebone.Pools;

namespace BareBone;

/// <summary>
/// Like <see cref="BBList{T}"/> but owns the contained items. Returns items to the Pool when removed/cleared etc.
/// </summary>
public class BBOwningList<T> : Poolable where T : Poolable, new()
{
    private T[] _items = [];

    protected internal override void Construct()
    {
        _items = [];
    }

    protected internal override void Destruct()
    {
        Clear(true, true);
    }

    public void Add(T item)
    {
        var idx = Count;
        if (idx < _items.Length)
        {
            Count = idx + 1;
            _items[idx] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    // Non-inline from List.Add to improve its code quality as uncommon path
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        Debug.Assert(Count == _items.Length);
        var count = Count;
        GrowCapacity(count + 1);
        Count = count + 1; // update Count AFTER Grow, because Grow may reallocate and copy all items based on Count.
        _items[count] = item;
    }

    /// <summary>
    /// Increase the capacity to at least the specified <paramref name="capacity"/>.
    /// </summary>
    private void GrowCapacity(int capacity)
    {
        Debug.Assert(_items.Length < capacity);

        var newCapacity = Math.Max(capacity, 2 * _items.Length);

        // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

        // If the computed capacity is still less than specified, set to the original argument.
        // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
        if (newCapacity < capacity) newCapacity = capacity;

        Capacity = newCapacity;
    }

    /// <summary>
    /// Fast remove of an item by moving the last item in the list over this deleted one. Returns the deleted item to the pool.
    /// </summary>
    /// <param name="returnToPool">Set to false when you only want to detach the items from the list, not return them to the pool.</param>
    public bool SwapRemove(T item, bool returnToPool = true)
    {
        var idx = IndexOf(item);
        if (idx <= -1) return false;

        SwapRemoveAt(idx, returnToPool);
        return true;
    }

    private int IndexOf(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        for (var i = 0; i < Count; i++)
        {
            if (item.Equals(_items[i])) return i;
        }

        return -1;
    }

    /// <summary>
    /// Fast remove of a range of items by copying over an equal amount of items from the back of the array (if applicable).
    /// </summary>
    /// <param name="returnToPool">Set to false when you only want to detach the items from the list, not return them to the pool.</param>
    public void SwapRemoveAt(int idx, bool returnToPool = true, int count = 1)
    {
        if (idx + count > Count) throw new ArgumentOutOfRangeException("The range to remove doesn't entirely fall within array bounds.");

        if (returnToPool)
            for (var i = 0; i < count; i++)
                Pool.Return(_items[idx + i]);

        if (idx + count < Count)
        {
            var sourceIdx = Count - count;
            sourceIdx = Math.Max(sourceIdx, idx + count); // ensure we only copy items AFTER the delete-range.
            Array.ConstrainedCopy(_items, sourceIdx, _items, idx, Count - sourceIdx);
        }

        Count -= count;
    }

    /// <summary>
    /// Removes the last item and returns it. More performant than SwapRemoveAt. Returns ownership of the item to the caller.
    /// </summary>
    public T Pop()
    {
        Count--;
        return _items[Count];
    }

    public T Last => _items[Count - 1];

    public void EnsureCapacity(int capacity)
    {
        if (Capacity >= capacity) return;
        GrowCapacity(capacity);
    }

    /// <summary>
    /// Clears the collection. Optionally frees the allocated capacity memory.
    /// </summary>
    public void Clear(bool destruct, bool freeCapacity)
    {
        if (destruct)
        {
            foreach (var item in AsReadOnlySpan())
                Pool.Return(item);
        }

        if (freeCapacity && _items.Length != 0)
        {
            ArrayPool<T>.Shared.Return(_items);
            _items = [];
        }
        Count = 0;
    }

    /// <summary>
    /// Returns a new array (copy) with the items in this GrowArray.
    /// </summary>
    public T[] ToArray()
    {
        return AsSpan().ToArray();
    }

    public int Capacity
    {
        get => _items.Length;
        private set
        {
            if (value < Count)
                throw new Exception("Cannot set capacity lower than items in this GrowArray.");

            if (value != _items.Length)
            {
                if (value > 0)
                {
                    var newItems = ArrayPool<T>.Shared.Rent(value);
                    if (Count > 0)
                    {
                        Array.Copy(_items, newItems, Count);
                    }
                    if (_items.Length > 0)
                        ArrayPool<T>.Shared.Return(_items);
                    _items = newItems;
                }
                else
                {
                    _items = [];
                }
            }
        }
    }

    public int Count { get; internal set; }

    /// <summary>
    /// The underlying array. Mind that the length of this array is not Count but Capacity. Also mind that altering this array bypasses the Destructing of items.
    /// </summary>
    public T[] InternalArray => _items;

    /// <summary>
    /// Direct access to the items as a Span.
    /// </summary>
    public Span<T> AsSpan() => _items.AsSpan(0, Count);

    /// <summary>
    /// Direct access to the items as a Span.
    /// </summary>
    public ReadOnlySpan<T> AsReadOnlySpan() => _items.AsSpan(0, Count);

    /// <summary>
    /// Direct access to the items as an <see cref="ArraySegment{T}"/>.
    /// </summary>
    public ArraySegment<T> AsArraySegment() => new(_items, 0, Count);
}