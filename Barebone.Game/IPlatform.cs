using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Geometry;

namespace Barebone.Game
{
    public interface IPlatform : IDisposable
    {
        void ProcessEvents(InputSubSystem input);
        bool IsQuitRequested { get; }
        void Present();

        IPlatformGraphics Graphics { get; }
        Vector2I GetWindowSize();
    }
}
