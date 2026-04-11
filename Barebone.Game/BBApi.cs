using Barebone.Game.Graphics;
using Barebone.Game.Input;

namespace Barebone.Game
{
    internal class BBApi(IClock clock, IGraphics graphics, IInput input) : IBBApi
    {
        public IClock Clock { get; } = clock;

        public IGraphics Graphics { get; } = graphics;
        public IInput Input { get; } = input;

        public void Quit()
        {
            QuitRequested = true;
        }

        public bool QuitRequested { get; private set; }
    }
}
