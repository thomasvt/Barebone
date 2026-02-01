namespace Barebone.Assets
{
    /// <summary>
    /// Store of unmanaged resources like textures, sounds that must be loaded once and then reused. Disposes all loaded resources if they implement IDisposable.
    /// </summary>
    public abstract class AssetStore<T> : IDisposable
    {
        private readonly Dictionary<string, T> _cache = new();

        /// <summary>
        /// Gets an asset by filename. The asset is loaded only once into a shared instance that is returned for each request.
        /// </summary>
        public T GetShared(string filename)
        {
            if (!_cache.TryGetValue(filename, out var resource))
            {
                resource = GetInstance(filename);
                _cache[filename] = resource;
            }
            return resource;
        }

        /// <summary>
        /// Gets multiple assets by name. Each asset is loaded only once into a shared instance that is returned for each request.
        /// </summary>
        public T[] GetSharedMany(params string[] filenames)
        {
            return filenames.Select(GetShared).ToArray();
        }

        /// <summary>
        /// Loads the asset from file into a new instance.
        /// </summary>
        public abstract T GetInstance(string filename);

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
