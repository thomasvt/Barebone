using Barebone.Graphics;

namespace Barebone.Assets
{
    public class TextureStore(string path, ITextureLoader textureLoader) : AssetStore<ITexture>, ITextureStore
    {
        public override ITexture GetInstance(string filename)
        {
            return textureLoader.LoadTexture(Path.Combine(path, filename));
        }
    }
}
