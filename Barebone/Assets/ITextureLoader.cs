using Barebone.Graphics;

namespace Barebone.Assets
{
    public interface ITextureLoader
    {
        ITexture LoadTexture(string path);
        ITexture LoadTexture(Stream stream);
    }
}
