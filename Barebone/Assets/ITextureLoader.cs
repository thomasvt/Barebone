using Barebone.Geometry;
using Barebone.Graphics;

namespace Barebone.Assets
{
    public interface ITextureLoader
    {
        ITexture LoadTexture(string path);
        ITexture LoadTexture(Stream stream);
        ITexture CreateTexture(Vector2I size);
    }
}
