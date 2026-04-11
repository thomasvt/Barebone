using Barebone.Game.Scene;
using Barebone.Geometry;

namespace Barebone.Game.Sdl
{
    public static class SdlEngine
    {
        public static void Run<TRootActor>(in string windowTitle, in Vector2I windowSize) where TRootActor: Actor, new()
        {
            using var platform = new SdlPlatform(windowTitle, windowSize);
            using var engine = new Engine(platform);
            engine.Run<TRootActor>();
        }
    }
}
