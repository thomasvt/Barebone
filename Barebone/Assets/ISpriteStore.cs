using Barebone.Graphics.Sprites;

namespace Barebone.Assets
{
    public interface ISpriteStore
    {
        Sprite GetInstance(string filename);
        float Scale { get; }

        /// <summary>
        /// Gets an asset by filename. The asset is loaded only once into a shared instance that is returned for each request.
        /// </summary>
        Sprite GetShared(string filename);

        /// <summary>
        /// Gets multiple assets by name. Each asset is loaded only once into a shared instance that is returned for each request.
        /// </summary>
        Sprite[] GetSharedMany(params string[] filenames);
    }
}
