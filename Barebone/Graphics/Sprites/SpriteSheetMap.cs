using Barebone.Geometry;

namespace Barebone.Graphics.Sprites
{
    public record struct SpriteSheetSprite(AabbI Aabb, Aabb AabbUV);

    public record SpriteSheetMap(SpriteSheetSprite[] Sprites)
    {
        /// <param name="spacing">Empty texels between sprits</param>
        /// <param name="borderPadding">Empty texels at the edges of the texture (no 'spacing' expected there)</param>
        public static SpriteSheetMap FromUniform(in Vector2I sheetSize, in Vector2I spriteSize, in int spacing, in int borderPadding)
        {
            var spriteCountX = (sheetSize.X + spacing) / (spriteSize.X + spacing);
            var spriteCountY = (sheetSize.Y + spacing) / (spriteSize.Y + spacing);
            var spriteCount = spriteCountX * spriteCountY;
            return FromUniform(sheetSize, spriteCount, spriteCountX, spriteSize, spacing, borderPadding);
        }

        /// <param name="spacing">Empty texels between sprits</param>
        /// <param name="borderPadding">Empty texels at the edges of the texture (no 'spacing' expected there)</param>
        public static SpriteSheetMap FromUniform(in Vector2I sheetSize, in int spriteCount, in int columnCount, in Vector2I spriteSize, in int spacing, in int borderPadding)
        {
            var sprites = new SpriteSheetSprite[spriteCount];
            var pos = new Vector2I(borderPadding);
            for (var i = 0; i < spriteCount; i++)
            {
                var aabb = new AabbI(pos, pos + spriteSize);
                var aabbUV = aabb.ToAabb() / sheetSize;
                sprites[i] = new SpriteSheetSprite(aabb, aabbUV);

                if (i % columnCount == columnCount - 1)
                {
                    pos.X = borderPadding;
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
