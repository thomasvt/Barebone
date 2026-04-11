using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Game.Scene;

namespace Barebone.Game
{
    public static class BB
    {
        internal static void Init(IClock clock, IDraw draw, ICamera camera, IInput input, IDebug debug, IPhysics physics)
        {
            Clock = clock;
            Draw = draw;
            Camera = camera;
            Input = input;
            Debug = debug;
            //Scene = scene;
            Physics = physics;
        }

        public static IClock Clock { get; private set; } = null!;
        public static IDraw Draw { get; private set; } = null!;
        public static ICamera Camera { get; private set; } = null!;
        public static IInput Input { get; private set; } = null!;
        public static IDebug Debug { get; private set; } = null!;
        // public static IScene Scene { get; private set; } = null!;
        public static IPhysics Physics { get; private set; } = null!;
        public static void Quit()
        {
            QuitRequested = true;
        }

        public static bool QuitRequested { get; private set; }
    }
}
