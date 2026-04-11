using Barebone.Game.Debug;
using Barebone.Game.Graphics;
using Barebone.Game.Input;

namespace Barebone.Game
{
    public interface IBBApi
    {
        IClock Clock { get; }
        IDraw Draw { get; }
        ICamera Camera { get; }
        IInput Input { get; }
        IDebug Debug { get; }
        void Quit();
    }
}
