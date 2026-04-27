using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Messaging;
using BareBone.Random;

namespace Barebone.Game
{
    public static class BB
    {
        internal static void Init(IClock clock, IGraphics graphics, IInput input, IDebug debug, IPhysics physics, IMessageBus messageBus)
        {
            Clock = clock;
            Graphics = graphics;
            Input = input;
            Debug = debug;
            Physics = physics;
            MessageBus = messageBus;
            Random = new StableRandom(1337);
        }

        public static IClock Clock { get; private set; } = null!;
        public static IGraphics Graphics { get; private set; } = null!;
        public static IInput Input { get; private set; } = null!;
        public static IDebug Debug { get; private set; } = null!;
        public static IPhysics Physics { get; private set; } = null!;
        public static IMessageBus MessageBus { get; private set; } = null!;
        public static StableRandom Random { get; private set; } = null!;
        public static void Quit()
        {
            QuitRequested = true;
        }

        public static bool QuitRequested { get; private set; }
    }
}
