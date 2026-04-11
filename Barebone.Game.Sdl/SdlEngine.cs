using Barebone.Geometry;

namespace Barebone.Game.Sdl
{
    public static class SdlEngine
    {
        public static void Run(in string windowTitle, in Vector2I windowSize, in IActor rootActor)
        {
            using var platform = new SdlPlatform(windowTitle, windowSize);
            var engine = new Engine(platform);
            engine.Run(rootActor);
        }
    }
}
