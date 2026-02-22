using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Graphics.Sprites
{
    public record struct SpriteSheetSprite(AabbI Aabb);

    public record SpriteSheetMap(SpriteSheetSprite[] Sprites)
    {
        public static SpriteSheetMap FromUniform(Vector2I sheetSize, Vector2I spriteSize, int spacing, int sheetBorder)
        {
            var spriteCountX = (sheetSize.X + spacing) / (spriteSize.X + spacing);
            var spriteCountY = (sheetSize.Y + spacing) / (spriteSize.Y + spacing);
            var spriteCount = spriteCountX * spriteCountY;
            return FromUniform(spriteCount, spriteCountX, spriteSize, spacing, sheetBorder);
        }

        /// <param name="spriteOrigin">Origin (or pivot point) of each sprite in sprite-local coords.</param>
        /// <param name="spacing">Empty texels between sprits</param>
        /// <param name="sheetBorder">Empty texels at the edges of the texture (no 'spacing' expected there)</param>
        public static SpriteSheetMap FromUniform(int spriteCount, int columnCount, Vector2I spriteSize,
            int spacing, int sheetBorder)
        {
            var sprites = new SpriteSheetSprite[spriteCount];
            var pos = new Vector2I(sheetBorder);
            for (var i = 0; i < spriteCount; i++)
            {
                var aabb = new AabbI(pos, pos + spriteSize);
                sprites[i] = new SpriteSheetSprite(aabb);

                if (i % columnCount == columnCount - 1)
                {
                    pos.X = sheetBorder;
                    pos.Y += spriteSize.Y + spacing;
                }
                else
                {
                    pos.X += spriteSize.X + spacing;
                }
            }

            return new SpriteSheetMap(sprites);
        }
    }
}
