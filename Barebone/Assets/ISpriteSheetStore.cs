using Barebone.Geometry;
using Barebone.Graphics.Sprites;

namespace Barebone.Assets
{
    public interface ISpriteSheetStore
    {
        SpriteSheet Load(string filename, Vector2I spriteSize, int spacing, int borderPadding, float scale);
    }
}
