namespace Barebone.Pools
{
    public interface IPoolable
    {
        /// <summary>
        /// Return this object to the pool it came from. Don't use the instance after calling this.
        /// </summary>
        void Return();
    }
}
