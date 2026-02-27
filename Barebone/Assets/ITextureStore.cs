using Barebone.Graphics;

namespace Barebone.Assets
{
    public interface ITextureStore
    {
        ITexture GetInstance(string filename);

        /// <summary>
        /// Gets an asset by filename. The asset is loaded only once into a shared instance that is returned for each request.
        /// </summary>
        ITexture GetShared(string filename);

        /// <summary>
        /// Gets multiple assets by name. Each asset is loaded only once into a shared instance that is returned for each request.
        /// </summary>
        ITexture[] GetSharedMany(params string[] filenames);
    }
}
