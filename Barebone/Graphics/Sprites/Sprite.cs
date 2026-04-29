using Barebone.Geometry;

namespace Barebone.Graphics.Sprites
{
    public record Sprite(ITexture Texture, Aabb UvCoords, Aabb AabbPx, bool IsTextureOwner) : IDisposable
    {
        public void Dispose()
        {
            if (IsTextureOwner)
                (Texture as IDisposable)?.Dispose();
        }
    }
}
