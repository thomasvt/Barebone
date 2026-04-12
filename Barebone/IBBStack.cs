using Barebone.Pools;

namespace Barebone
{
    /// <summary>
    /// Stack without GC pressure and with BB Pooling support.
    /// </summary>
    public interface IBBStack<T> : IPoolable, IDisposable
    {
        /// <summary>
        /// Adds an item to this stack.
        /// </summary>
        void Push(T item);

        /// <summary>
        /// Removes the top item of this stack and returns it.
        /// </summary>
        T Pop();

        /// <summary>
        /// Returns the top item of the stack without removing it.
        /// </summary>
        T Peek();

        /// <summary>
        /// Clears the collection. Optionally frees the allocated capacity memory.
        /// </summary>
        /// <param name="freeCapacity">free the memory allocated for the current capacity of this GrowArray</param>
        void Clear(bool returnItems, bool freeCapacity);

        int Count { get; }

        /// <summary>
        /// Direct access to the items as a Span.
        /// </summary>
        ReadOnlySpan<T> AsReadOnlySpan();

        /// <summary>
        /// Returns this BBList after returning all items that are <see cref="IPoolable"/>. 
        /// </summary>
        /// <param name="returnItems">Also returns all contained items to the pool when applicable.</param>
        void Return(bool returnItems);

        void DisposeItems();
    }
}
