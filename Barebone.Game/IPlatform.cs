using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Geometry;

namespace Barebone.Game
{
    public interface IPlatform : IDisposable
    {
        void ProcessPlatformEvents(InputSubSystem input);

        /// <summary>
        /// Stops the game's process.
        /// </summary>
        void Quit();

        IPlatformGraphics Graphics { get; }
        Vector2I GetWindowSize();
    }
}
