namespace Barebone.Pools
{
    /// <summary>
    /// Similar to the Disposable pattern but for pooled objects. Lets the class implement Construct() and Destruct()
    /// which are called upon an instance being rented from and returned to the pool.
    /// </summary>
    public abstract class Poolable : IPoolable
    {
        internal IPool OriginPool { get; set; }

        /// <summary>
        /// Is this instance in use by the application (true) or an unused instance sitting in the 'free' pool (false).
        /// </summary>
        internal bool IsRented { get; set; }

        /// <summary>
        /// Called when rented from the pool. Should initialize state and rent sub-objects.
        /// </summary>
        protected internal abstract void Construct();

        /// <summary>
        /// Called when returned to the pool. Should fully clear state (fields) and return all rented sub-objects.
        /// </summary>
        protected internal abstract void Destruct();

        /// <summary>
        /// Return this object to the pool it came from. Don't use the instance after calling this.
        /// </summary>
        public void Return()
        {
            OriginPool.Return(this);
        }
    }
}
