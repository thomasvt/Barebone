using Barebone.Game.Core;
using Barebone.Geometry;

namespace Barebone.Game.Sdl
{
    public static class SdlEngine
    {
        public static void Run(in IActor rootActor, in string windowTitle, in Vector2I windowSize, in Vector2I? viewSize = null, in LogicalScaleMode scaleMode = LogicalScaleMode.Disabled)
        {
            using var platform = new SdlPlatform(windowTitle, windowSize, viewSize ?? windowSize, scaleMode);
            var engine = new Engine(platform);
            engine.Run(rootActor);
        }
    }
}
