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
            SpriteCount = map.Sprites.Length;
            var sprites = new List<Sprite>(SpriteCount);
            Sprites = sprites;
            for (var i = 0; i < SpriteCount; i++)
            {
                var s = map.Sprites[i];
                // reverse Y because UVs are Y+ = down, while world is Y+ = up.
                var aabbUV = new Aabb(s.AabbUV.MinCorner.X, s.AabbUV.MaxCorner.Y, s.AabbUV.MaxCorner.X, s.AabbUV.MinCorner.Y);

                sprites.Add(new Sprite(texture, aabbUV, Aabb.FromSizeAroundCenter(s.Aabb.Size * scale), false));
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
