using Barebone.Game.Graphics;

namespace Barebone.Game
{
    public interface IBBApi
    {
        IClock Clock { get; }
        IGraphics Graphics { get; }
        void Quit();
    }
}
