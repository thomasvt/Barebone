using Barebone.Geometry;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    internal record XnaTexture(Texture2D Texture) : ITexture, IDisposable
    {
        public void Dispose()
        {
            Texture.Dispose();
        }

        public Vector2I Size => new(Texture.Width, Texture.Height);

        public void ReadPixels(in Color8[] pixelBuffer)
        {
            Texture.GetData(pixelBuffer);
        }

        public void WritePixels(in Color8[] pixelBuffer)
        {
            Texture.SetData(pixelBuffer);
        }
    }
}
