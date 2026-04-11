using Barebone.Game.Graphics;
using Barebone.Game.Input;

namespace Barebone.Game
{
    public interface IBBApi
    {
        IClock Clock { get; }
        IDraw Draw { get; }
        IInput Input { get; }
        void Quit();
    }
}
