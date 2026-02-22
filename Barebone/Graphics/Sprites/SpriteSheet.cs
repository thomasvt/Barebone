using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Graphics.Sprites
{
    /// <summary>
    /// Horizontal-only sheet of sprites. With Spacing between each sprite.
    /// </summary>
    public class SpriteSheet
        : IDisposable
    {
        private readonly ITexture _texture;

        public SpriteSheet(ITexture texture, SpriteSheetMap map, float scale)
        {
            _texture = texture;
            MapData = map;

            var sprites = new List<Sprite>();
            Sprites = sprites;
            for (var i = 0; i < SpriteCount; i++)
            {
                var spriteMap = Sprites[i];
                var spriteUv = spriteMap.AabbPx / texture.Size;

                sprites.Add(new Sprite(texture, spriteUv, Aabb.FromSizeAroundCenter(spriteMap.AabbPx.Size * scale), false));
            }
        }

        public IReadOnlyList<Sprite> Sprites { get; }
        public int SpriteCount { get; }
        public SpriteSheetMap MapData { get; }
        public Sprite this[int idx] => Sprites[idx];

        public void Dispose()
        {
            (_texture as IDisposable)?.Dispose();
        }
    }
}
