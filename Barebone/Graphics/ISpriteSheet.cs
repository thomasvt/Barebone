namespace Barebone.Graphics
{
    public interface ISpriteSheet
    {
        int SpriteCount {get;}
        ISprite this[int idx] { get; }
    }
}
