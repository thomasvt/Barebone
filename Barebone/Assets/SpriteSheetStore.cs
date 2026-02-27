using Barebone.Geometry;
using Barebone.Graphics.Sprites;

namespace Barebone.Assets
{
    public class SpriteSheetStore(ISpriteStore spriteStore) : ISpriteSheetStore
    {
        public SpriteSheet Load(string filename, Vector2I spriteSize, int spacing, int borderPadding, float scale)
        {
            var sprite = spriteStore.GetShared(filename);
            var map = SpriteSheetMap.FromUniform(sprite.Texture.Size, spriteSize, spacing, borderPadding);
            return new SpriteSheet(sprite.Texture, map, scale);
        }
    }
}
