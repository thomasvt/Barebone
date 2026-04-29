using Barebone.Pools;

namespace Barebone
{
    /// <summary>
    /// A BBList that defers all mutations to the controlled moment when ApplyChanges() is called. Removals maintain order of the remaining items.
    /// Can be used both in Poolable or classic IDisposable pattern.
    /// </summary>
    public sealed class BBListDeferred<T> : Poolable, IDisposable
    {
        internal enum MutationType { Add, Remove }

        internal readonly record struct Mutation(MutationType Type, T? Item);

        private BBList<T> _list = null!;
        private IBBQueue<Mutation> _queue = null!;

        public BBListDeferred()
        {
            Construct();
        }

        protected internal override void Construct()
        {
            _list?.Return();
            _list = Pool.Rent<BBList<T>>();

            _queue?.Return();
            _queue = Pool.Rent<BBList<Mutation>>().AsQueue();
        }

        protected internal override void Destruct()
        {
            _list.Return();
            _list = null!;
            _queue.Return();
            _queue = null!;
        }

        /// <summary>
        /// Enqueues this item to be added the next time you call ApplyChanges().
        /// </summary>
        public void Add(T item)
        {
            _queue.Enqueue(new(MutationType.Add, item));
        }

        /// <summary>
        /// Enqueues this item to be removed the next time you call ApplyChanges().
        /// </summary>
        public void Remove(T item)
        {
            _queue.Enqueue(new(MutationType.Remove, item));
        }

        /// <summary>
        /// Immediately clears all items including pending changes that are not yet applied.
        /// </summary>
        public void ClearImmediate(bool returnPoolableItems, bool freeCapacity)
        {
            _list.DisposeItems();
            _list.Clear(returnPoolableItems, freeCapacity);
            _queue.DisposeItems();
            _queue.Clear(returnPoolableItems, freeCapacity);
        }

        /// <summary>
        /// Applies all deferred mutations to the list.
        /// </summary>
        public void ApplyChanges(bool returnPoolableItems)
        {
            while (_queue.Count > 0)
            {
                var command = _queue.Dequeue();
                switch (command.Type)
                {
                    case MutationType.Add:
                        _list.Add(command.Item!);
                        OnAdded?.Invoke(command.Item!);
                        break;
                    case MutationType.Remove:
                        OnRemoving?.Invoke(command.Item!);
                        _list.Remove(command.Item!, returnPoolableItems);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Returns the count items, disregarding mutations that are not yet applied.
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Direct access to the items as a Span.
        /// </summary>
        public Span<T> AsSpan() => _list.AsSpan();

        /// <summary>
        /// Direct access to the items as a Span.
        /// </summary>
        public ReadOnlySpan<T> AsReadOnlySpan() => _list.AsReadOnlySpan();

        /// <summary>
        /// Checks if the item is in this list, disregarding mutations that are not yet applied.
        /// </summary>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// Returns this list to the pool.
        /// </summary>
        /// <param name="returnItems">Also returns all contained items to the pool when applicable.</param>
        public void Return(bool returnItems)
        {
            _queue.Return(returnItems);
            _list.Return(returnItems);
        }

        /// <summary>
        /// Calls Dispose on all items that implement IDisposable.
        /// </summary>
        public void DisposeItems()
        {
            _queue.DisposeItems();
            _list.DisposeItems();
        }

        public void Dispose()
        {
            Destruct();
        }

        public Action<T>? OnAdded { get; set; }
        public Action<T>? OnRemoving { get; set; }
    }
}
