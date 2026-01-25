using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics.Sprites;

namespace Barebone.Assets
{
    public class SpriteStore(float scale, TextureStore textureStore) : AssetStore<Sprite>
    {
        public override Sprite Load(string filename)
        {
            var texture = textureStore.Get(filename);
            return new Sprite(texture, new Aabb(0, 1, 1, 0),  Aabb.FromSizeAroundCenter(texture.Size) * scale, false);
        }
    }
}
