using Barebone.Game.Graphics;

namespace Barebone.Game
{
    internal class BBApi(IClock clock, IGraphics graphics) : IBBApi
    {
        public IClock Clock { get; } = clock;

        public IGraphics Graphics { get; } = graphics;

        public void Quit()
        {
            QuitRequested = true;
        }

        public bool QuitRequested { get; private set; }
    }
}
