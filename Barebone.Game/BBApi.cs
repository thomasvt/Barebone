using Barebone.Game.Debug;
using Barebone.Game.Graphics;
using Barebone.Game.Input;

namespace Barebone.Game
{
    internal class BBApi(IClock clock, IDraw draw, ICamera camera, IInput input, IDebug debug) : IBBApi
    {
        public IClock Clock { get; } = clock;

        public IDraw Draw { get; } = draw;
        public ICamera Camera { get; } = camera;
        public IInput Input { get; } = input;
        public IDebug Debug { get; } = debug;

        public void Quit()
        {
            QuitRequested = true;
        }

        public bool QuitRequested { get; private set; }
    }
}
