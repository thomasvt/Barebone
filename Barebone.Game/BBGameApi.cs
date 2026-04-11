using Barebone.Game.Graphics;
using Barebone.Game.Input;

namespace Barebone.Game
{
    public interface IBBApi
    {
        IClock Clock { get; }
        IDraw Draw { get; }

        /// <summary>
        /// Alter or read the Camera. The camera is frozen during Draw, so you must alter it in the Update phase.
        /// </summary>
        ICamera Camera { get; }
        IInput Input { get; }
        void Quit();
    }
}
