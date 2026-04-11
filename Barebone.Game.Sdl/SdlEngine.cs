using Barebone.Game.Core;
using Barebone.Game.Graphics;
using Barebone.Geometry;

namespace Barebone.Game.Sdl
{
    public static class SdlEngine
    {
        public static void Run(in IActor rootActor, in string windowTitle, in Vector2I windowSize)
        {
            using var platform = new SdlPlatform(windowTitle, windowSize);
            var engine = new Engine(platform);
            engine.Run(rootActor);
        }
    }
}
