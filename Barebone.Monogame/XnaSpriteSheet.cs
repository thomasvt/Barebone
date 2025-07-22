using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    /// <summary>
    /// Horizontal-only sheet of sprites. With Spacing between each sprite.
    /// </summary>
    public class XnaSpriteSheet
        : IDisposable, ISpriteSheet
    {
        private readonly Texture2D? _texture;

        public XnaSpriteSheet(GraphicsDevice graphicsDevice, string file, SpriteSheetMeta meta)
        {
            MetaData = meta;
            _texture = Texture2D.FromFile(graphicsDevice, file);
            SpriteCount = (_texture.Width + meta.Spacing) / (meta.SpriteSize.X + meta.Spacing);

            var spriteSizeUv =new Vector2(meta.SpriteSize.X / (float)_texture.Width, 1f);
            var spriteStrideUv = new Vector2((meta.SpriteSize.X + meta.Spacing) / (float)_texture.Width, 1f);

            var sprites = new List<ISprite>();
            Sprites = sprites;
            for (var i = 0; i < SpriteCount; i++)
            {
                // convert bitmap coords (Y+ is down) to world coords (Y+ is up)
                var originWorldCoords = new Vector2(meta.SpriteOrigin.X, meta.SpriteSize.Y - meta.SpriteOrigin.Y);
                var quad = new Aabb(Vector2.Zero, meta.SpriteSize) - originWorldCoords;

                var bottomLeftUV = new Vector2(spriteStrideUv.X * i, spriteSizeUv.Y);
                var topRightUV = new Vector2(bottomLeftUV.X + spriteSizeUv.X, 0);
                var quadUV = new Aabb(bottomLeftUV, topRightUV);

                sprites.Add(new XnaSprite(_texture, quadUV, quad, false));
            }
        }

        public IReadOnlyList<ISprite> Sprites { get; }
        public int SpriteCount { get; }
        public SpriteSheetMeta MetaData { get; }
        public ISprite this[int idx] => Sprites[idx];

        public void Dispose()
        {
            _texture?.Dispose();
        }
    }
}
