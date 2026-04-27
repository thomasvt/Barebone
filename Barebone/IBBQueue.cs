using Barebone.Pools;

namespace Barebone
{
    /// <summary>
    /// Queueu without GC pressure and with BB Pooling support.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBBQueue<T> : IPoolable, IDisposable
    {
        void Enqueue(T item);

        T Dequeue();

        /// <summary>
        /// Returns the top item of the queue without removing it.
        /// </summary>
        public T Peek();

        /// <summary>
        /// Clears the collection. Optionally frees the allocated capacity memory.
        /// </summary>
        /// <param name="freeCapacity">free the memory allocated for the current capacity of this GrowArray</param>
        public void Clear(bool returnItems = false, bool freeCapacity = false);

        public int Count { get; }

        /// <summary>
        /// Direct access to the items as a Span.
        /// </summary>
        public ReadOnlySpan<T> AsReadOnlySpan();

        /// <summary>
        /// Returns this BBList after returning all items that are <see cref="IPoolable"/>. 
        /// </summary>
        /// <param name="returnItems">Also returns all contained items to the pool when applicable.</param>
        public void Return(bool returnItems);

        public void DisposeItems();

        /// <summary>
        /// Reverses the entire queue.
        /// </summary>
        void Reverse();
    }
}
