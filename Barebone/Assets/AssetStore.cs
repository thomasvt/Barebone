namespace Barebone.Assets
{
    /// <summary>
    /// Store of unmanaged resources like textures, sounds that must be loaded once and then reused. Disposes all loaded resources if they implement IDisposable.
    /// </summary>
    public abstract class AssetStore<T> : IDisposable
    {
        private readonly Dictionary<string, T> _cache = new();

        public T Get(string filename)
        {
            if (!_cache.TryGetValue(filename, out var resouce))
            {
                resouce = Load(filename);
                _cache[filename] = resouce;
            }
            return resouce;
        }

        public T[] GetMany(params string[] names)
        {
            return names.Select(Get).ToArray();
        }

        protected abstract T Load(string filename);

        public void Dispose()
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T))) 
            {
                foreach (var resource in _cache.Values)
                {
                    ((IDisposable)resource!).Dispose();
                }
            }
        }
    }
}
