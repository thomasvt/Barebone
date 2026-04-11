using Barebone.Game.Graphics;
using Barebone.Game.Input;

namespace Barebone.Game
{
    internal class BBApi(IClock clock, IDraw draw, IInput input) : IBBApi
    {
        public IClock Clock { get; } = clock;

        public IDraw Draw { get; } = draw;
        public IInput Input { get; } = input;

        public void Quit()
        {
            QuitRequested = true;
        }

        public bool QuitRequested { get; private set; }
    }
}
