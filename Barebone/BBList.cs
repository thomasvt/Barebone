using System.Buffers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Barebone.Pools;

namespace Barebone
{
    /// <summary>
    /// List-like class with zero GC pressure and several faster alternatives than List by sacrificing the stable item-order and exposing the internal array.
    /// Supports both Poolable and normal Disposable patterns.
    /// Fast alternatives are:
    /// * use as unordered but fast queue using Add() and Pop() for cases where the pop order is irrelevant.
    /// * SwapRemove: removes a range of items by moving items from the back of the list over the range to remove.
    /// * AddSpan: performs ranged memory-copy instead of items 1 by 1.
    /// </summary>
    public class BBList<T> : Poolable, IBBQueue<T>, IBBStack<T>, IDisposable
    {
        private T[] _items = [];
        public int Count { get; private set; } = 0;

        protected internal override sealed void Construct()
        {
            _items = [];
            Count = 0;
        }

        protected internal override void Destruct()
        {
            if (_items.Length > 0)
                ArrayPool<T>.Shared.Return(_items);
            _items = [];
            Count = 0;
        }

        public int Add(in T item)
        {
            var idx = Count;
            if (idx < _items.Length)
            {
                Count = idx + 1;
                _items[idx] = item;
                return idx;
            }
            return AddWithResize(item);
        }

        /// <summary>
        /// Inserts an item at the beginning of this BBList. Combine with Dequeue() to us this List as a queue.
        /// Note that you can only use Enqueue and Dequeue if you don't want the items' order to change.
        /// </summary>
        public void Enqueue(T item)
        {
            if (Count == _items.Length)
                GrowCapacity(Count + 1);

            if (Count > 0)
                Array.Copy(_items, 0, _items, 1, Count);
            _items[0] = item;
            Count++;
        }

        /// <summary>
        /// Gets the last item on the list and removes it. Combine with Enqueue() to use this List as a queue.
        /// Note that you can only use Enqueue and Dequeue if you don't want the items' order to change.
        /// </summary>
        public T Dequeue()
        {
            Count--;
            return _items[Count];
        }


        /// <summary>
        /// Inserts at the given index.
        /// </summary>
        public void InsertAt(int idx, T item)
        {
            if (idx > Count) throw new ArgumentOutOfRangeException(nameof(idx));

            if (idx == Count)
            {
                Add(item);
            }
            else
            {
                if (Count == _items.Length)
                    GrowCapacity(Count + 1);

                var moveCount = Count - idx;
                if (moveCount > 0)
                    Array.Copy(_items, idx, _items, idx + 1, moveCount);
                _items[idx] = item;
                Count++;
            }
        }

        /// <summary>
        /// Copies the items to the end of this BBList in a single copy operation.
        /// </summary>
        public void AddArray(T[] items)
        {
            EnsureCapacity(Count + items.Length);
            items.CopyTo(_items.AsSpan()[Count..]);
            Count += items.Length;
        }

        /// <summary>
        /// Adds the items one by one to the end of this BBList.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (items is IReadOnlyCollection<T> collection)
                EnsureCapacity(Count + collection.Count);

            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Copies the items to the end of this BBList in a single copy operation.
        /// </summary>
        public void AddSpan(ReadOnlySpan<T> items)
        {
            EnsureCapacity(Count + items.Length);
            items.CopyTo(_items.AsSpan()[Count..]);
            Count += items.Length;
        }

        /// <summary>
        /// Copies the items to the end of this BBList in a single copy operation.
        /// </summary>
        public void AddBBList(BBList<T> bbList)
        {
            var items = bbList.AsReadOnlySpan();
            EnsureCapacity(Count + items.Length);
            items.CopyTo(_items.AsSpan()[Count..]);
            Count += items.Length;
        }

        /// <summary>
        /// Adds uninitialized items to the GrowArray and returns a Span that contains them.
        /// </summary>
        public Span<T> AddNonInitialized(int count)
        {
            var idx = Count;
            var newCount = idx + count;

            if (newCount > _items.Length)
                GrowCapacity(newCount);

            Count = newCount; // update Count AFTER Grow, because Grow may reallocate and copy all items based on Count.

            return _items.AsSpan(idx, count);
        }

        // Non-inline from List.Add to improve its code quality as uncommon path

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int AddWithResize(in T item)
        {
            Debug.Assert(Count == _items.Length);
            var count = Count;
            GrowCapacity(count + 1);
            Count = count + 1; // update Count AFTER Grow, because Grow may reallocate and copy all items based on Count.
            _items[count] = item;
            return count;
        }

        /// <summary>
        /// Increase the capacity to at least the specified <paramref name="capacity"/>.
        /// </summary>
        private void GrowCapacity(in int capacity)
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
        /// Fast remove of an item by overwriting it with the last item and shrinking the array by 1.
        /// This sacrifices the order of the items in the array but is faster because no parts of the array need to be copied around to close the gap.
        /// </summary>
        public bool SwapRemove(T item, bool returnIfPoolable = false)
        {
            var idx = IndexOf(item);
            if (idx <= -1) return false;

            SwapRemoveRange(idx, 1, returnIfPoolable);
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
        /// This is faster than filling the gap by copying a larger part of the array to maintaining original item-order.
        /// </summary>
        public void SwapRemoveRange(int idx, int count = 1, bool returnIfPoolable = false)
        {
            if (idx + count > Count)
                throw new ArgumentOutOfRangeException("The range to remove doesn't entirely fall within array bounds.");

            if (returnIfPoolable)
                foreach (var item in AsReadOnlySpan().Slice(idx, count))
                    (item as IPoolable)?.Return();

            if (idx + count < Count)
            {
                var sourceIdx = Count - count;
                sourceIdx = Math.Max(sourceIdx, idx + count); // ensure we only copy items AFTER the delete-range.
                Array.ConstrainedCopy(_items, sourceIdx, _items, idx, Count - sourceIdx);
            }

            Count -= count;
        }

        /// <summary>
        /// Fast remove of a range of items by copying over an equal amount of items from the back of the array (if applicable).
        /// This is faster than filling the gap by copying a larger part of the array to maintaining original item-order.
        /// </summary>
        public void SwapRemoveAt(int idx, bool returnIfPoolable = false)
        {
            if (idx >= Count)
                throw new ArgumentOutOfRangeException(nameof(idx), "Index out of range.");

            if (returnIfPoolable)
                (_items[idx] as IPoolable)?.Return();

            if (idx < Count - 1)
                _items[idx] = _items[Count - 1];

            Count--;
        }

        /// <summary>
        /// Adds an item to this BBList. Combine with Pop() and Peek() to use as a stack.
        /// </summary>
        public void Push(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Removes the last item and returns it. More performant than SwapRemoveAt.
        /// </summary>
        public T Pop()
        {
            Count--;
            return _items[Count];
        }

        public void EnsureCapacity(int capacity)
        {
            if (Capacity >= capacity) return;
            GrowCapacity(capacity);
        }

        /// <summary>
        /// Clears the collection. Optionally frees the allocated capacity memory.
        /// </summary>
        /// <param name="returnItems">Also returns all contained items to the pool when applicable.</param>
        /// <param name="freeCapacity">free the memory allocated for the current capacity of this GrowArray</param>
        public void Clear(bool returnItems = false, bool freeCapacity = false)
        {
            if (returnItems)
                foreach (var item in AsReadOnlySpan())
                    (item as IPoolable)?.Return();

            if (freeCapacity)
            {
                ArrayPool<T>.Shared.Return(_items);
                _items = [];
            }

            Count = 0;
        }

        /// <summary>
        /// Returns this BBList after returning all items that are <see cref="IPoolable"/>. 
        /// </summary>
        /// <param name="returnItems">Also returns all contained items to the pool when applicable.</param>
        public void Return(bool returnItems)
        {
            if (returnItems)
                Clear(true, false);
            Return();
        }

        /// <summary>
        /// Returns a new array (copy) with the items in this GrowArray.
        /// </summary>
        public T[] ToArray()
        {
            return AsReadOnlySpan().ToArray();
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

        /// <summary>
        /// The underlying array. Mind that the length of this array is often bigger than Count because it's the Capacity of this GrowArray.
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

        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            foreach (var i in AsSpan())
            {
                if (item.Equals(i)) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the top item of the stack or queue when this BBList is used as such.
        /// </summary>
        public T Peek()
        {
            if (Count == 0) throw new Exception("Cannot peek on an empty BBList");
            return InternalArray[Count - 1];
        }

        /// <summary>
        /// Sets the list to a fixed number of items. Existing items within that range remain,
        /// New slots are initialized with default values if 'clearNewSlots' is true.
        /// </summary>
        public void SetFixedCount(int count, bool clearNewSlots)
        {
            EnsureCapacity(count);
            var oldCount = Count;
            Count = count;
            if (oldCount < Count)
                AsSpan()[oldCount..].Clear();
        }

        public void Reverse()
        {
            Array.Reverse(_items, 0, Count);
        }

        /// <summary>
        /// Allows to use this BBList as a queue only. This prevents you from using queue-breaking features of BBList.
        /// </summary>
        public IBBQueue<T> AsQueue() => this;

        /// <summary>
        /// Allows to use this BBList as a stack only. This prevents you from using stack-breaking features of BBList.
        /// </summary>
        public IBBStack<T> AsStack() => this;

        public void DisposeItems()
        {
            foreach (var item in AsReadOnlySpan())
                (item as IDisposable)?.Dispose();
        }

        public void Dispose()
        {
            Destruct();
        }
    }
}