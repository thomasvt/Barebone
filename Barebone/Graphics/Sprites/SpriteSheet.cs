using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Graphics.Sprites
{
    /// <summary>
    /// Horizontal-only sheet of sprites. With Spacing between each sprite.
    /// </summary>
    public class XnaSpriteSheet
        : IDisposable
    {
        private readonly ITexture _texture;

        public XnaSpriteSheet(ITexture texture, SpriteSheetMeta meta)
        {
            _texture = texture;
            MetaData = meta;
            SpriteCount = (texture.Size.X + meta.Spacing) / (meta.SpriteSize.X + meta.Spacing);

            var spriteSizeUv =new Vector2(meta.SpriteSize.X / (float)texture.Size.X, 1f);
            var spriteStrideUv = new Vector2((meta.SpriteSize.X + meta.Spacing) / (float)texture.Size.X, 1f);

            var sprites = new List<Sprite>();
            Sprites = sprites;
            for (var i = 0; i < SpriteCount; i++)
            {
                // convert bitmap coords (Y+ is down) to world coords (Y+ is up)
                var originWorldCoords = new Vector2(meta.SpriteOrigin.X, meta.SpriteSize.Y - meta.SpriteOrigin.Y);
                var quad = new Aabb(Vector2.Zero, meta.SpriteSize) - originWorldCoords;

                var bottomLeftUV = new Vector2(spriteStrideUv.X * i, spriteSizeUv.Y);
                var topRightUV = new Vector2(bottomLeftUV.X + spriteSizeUv.X, 0);
                var quadUV = new Aabb(bottomLeftUV, topRightUV);

                sprites.Add(new Sprite(texture, quadUV, quad, false));
            }
        }

        public IReadOnlyList<Sprite> Sprites { get; }
        public int SpriteCount { get; }
        public SpriteSheetMeta MetaData { get; }
        public Sprite this[int idx] => Sprites[idx];

        public void Dispose()
        {
            (_texture as IDisposable)?.Dispose();
        }
    }
}
