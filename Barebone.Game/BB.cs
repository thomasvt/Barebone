using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Geometry;

namespace Barebone.Game
{
    public static class BB
    {
        internal static void Init(IClock clock, IGraphics graphics, IInput input, IDebug debug, IPhysics physics)
        {
            Clock = clock;
            Graphics = graphics;
            Input = input;
            Debug = debug;
            Physics = physics;
        }

        public static IClock Clock { get; private set; } = null!;
        public static IGraphics Graphics { get; private set; } = null!;
        public static IInput Input { get; private set; } = null!;
        public static IDebug Debug { get; private set; } = null!;
        public static IPhysics Physics { get; private set; } = null!;
        public static void Quit()
        {
            QuitRequested = true;
        }

        public static bool QuitRequested { get; private set; }
    }
}
