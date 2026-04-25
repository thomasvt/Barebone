using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Game.Monogame
{
    /// <summary>
    /// Wraps a <see cref="Texture2D"/> as a Barebone <see cref="ITexture"/>.
    /// </summary>
    public class MonoGameTexture : ITexture, IDisposable
    {
        public Texture2D Texture { get; }
        public Vector2I Size { get; }

        public MonoGameTexture(Texture2D texture)
        {
            Texture = texture;
            Size = new Vector2I(texture.Width, texture.Height);
        }

        public void ReadPixels(in ColorRgba[] pixelBuffer)
        {
            var count = Size.X * Size.Y;
            if (pixelBuffer.Length < count) throw new ArgumentException("pixelBuffer must be large enough to hold all pixels of the texture.");
            // Texture2D.GetData<T> works with arrays/spans of unmanaged structs of the same byte layout.
            // ColorRgba is 4 bytes (R,G,B,A), matching MonoGame's Color memory layout (also RGBA byte-packed).
            Texture.GetData(pixelBuffer, 0, count);
        }

        public void WritePixels(in ColorRgba[] pixelBuffer)
        {
            var count = Math.Min(pixelBuffer.Length, Size.X * Size.Y);
            Texture.SetData(pixelBuffer, 0, count);
        }

        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}
