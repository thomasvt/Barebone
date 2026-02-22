using Barebone.Graphics.Sprites;

namespace Barebone.Assets
{
    public class SpriteSheetStore(SpriteStore spriteStore) : AssetStore<SpriteSheet>
    {
        public override SpriteSheet GetInstance(string filename)
        {
            var sprite = spriteStore.GetShared(filename);
            return new SpriteSheet(sprite.Texture, SpriteSheetMap.FromUniform(sprite.Texture.Size, new(16), 1, 1), spriteStore.Scale);
        }
    }
}
