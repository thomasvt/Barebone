using Barebone.Graphics;

namespace Barebone.Assets
{
    public class TextureStore(string path, ITextureLoader textureLoader) : AssetStore<ITexture>
    {
        protected override ITexture Load(string filename)
        {
            return textureLoader.LoadTexture(Path.Combine(path, filename));
        }
    }
}
