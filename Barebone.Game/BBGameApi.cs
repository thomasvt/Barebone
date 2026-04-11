using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Game.Scene;

namespace Barebone.Game
{
    public interface IBBApi
    {
        IClock Clock { get; }
        IDraw Draw { get; }
        ICamera Camera { get; }
        IInput Input { get; }
        IDebug Debug { get; }
        IScene Scene { get; }
        IPhysics Physics { get; }
        void Quit();
    }
}
