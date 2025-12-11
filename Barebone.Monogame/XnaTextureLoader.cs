using Barebone.Assets;
using Barebone.Geometry;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    internal class XnaTextureLoader(GraphicsDevice graphicsDevice) : ITextureLoader
    {
        public ITexture LoadTexture(string path)
        {
            return new XnaTexture(Texture2D.FromFile(graphicsDevice, path));
        }

        public ITexture LoadTexture(Stream stream)
        {
            return new XnaTexture(Texture2D.FromStream(graphicsDevice, stream));
        }

        public ITexture CreateTexture(Vector2I size)
        {
            return new XnaTexture(new Texture2D(graphicsDevice, size.X, size.Y));
        }
    }
}
