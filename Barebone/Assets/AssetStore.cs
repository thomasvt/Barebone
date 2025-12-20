namespace Barebone.Assets
{
    /// <summary>
    /// Store of unmanaged resources like textures, sounds that must be loaded once and then reused. Disposes all loaded resources if they implement IDisposable.
    /// </summary>
    public abstract class AssetStore<T> : IDisposable
    {
        private readonly Dictionary<string, T> _cache = new();

        /// <summary>
        /// Loads or reuses the previously loaded asset with the given name.
        /// </summary>
        public T Get(string name)
        {
            if (!_cache.TryGetValue(name, out var resouce))
            {
                resouce = Load(name);
                _cache[name] = resouce;
            }
            return resouce;
        }

        /// <summary>
        /// Gets multiple assets by name. Reuses already loaded assets when available.
        /// </summary>
        public T[] GetMany(params string[] names)
        {
            return names.Select(Get).ToArray();
        }

        /// <summary>
        /// Loads the asset from file.
        /// </summary>
        public abstract T Load(string filename);

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
