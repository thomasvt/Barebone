using Barebone.Geometry;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    internal record XnaSprite(Texture2D Texture, Aabb UvCoords, Aabb AabbPx, bool IsTextureOwner) : ISprite
    {
        public void Dispose()
        {
            if (IsTextureOwner)
                Texture.Dispose();
        }
    }
}
