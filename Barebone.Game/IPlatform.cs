using Barebone.Game.Graphics;

namespace Barebone.Game
{
    public interface IPlatform : IDisposable
    {
        void ProcessEvents();
        bool ShouldQuit { get; }
        void Present();

        IPlatformGraphics Graphics { get; }
    }
}
