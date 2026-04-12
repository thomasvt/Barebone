using Barebone.Geometry;

namespace Barebone.Game.Sdl
{
    public static class SdlEngine
    {
        public static void Run(in Func<IGame> gameFactory, in string windowTitle, in Vector2I windowSize)
        {
            using var platform = new SdlPlatform(windowTitle, windowSize);
            using var engine = new Engine(platform);
            engine.Run(gameFactory);
        }
    }
}
