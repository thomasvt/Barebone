using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Messaging;
using Barebone.Random;

namespace Barebone.Game
{
    public static class BB
    {
        private static IPlatform _platform = null!;

        internal static void Init(IClock clock, IGraphics graphics, IInput input, IDebug debug, IPhysics physics, IMessageBus messageBus, IPlatform platform)
        {
            Clock = clock;
            Graphics = graphics;
            Input = input;
            Debug = debug;
            Physics = physics;
            MessageBus = messageBus;
            Rng = new RngStream(1337);
            _platform = platform;
        }

        public static IClock Clock { get; private set; } = null!;
        public static IGraphics Graphics { get; private set; } = null!;
        public static IInput Input { get; private set; } = null!;
        public static IDebug Debug { get; private set; } = null!;
        public static IPhysics Physics { get; private set; } = null!;
        public static IMessageBus MessageBus { get; private set; } = null!;
        public static RngStream Rng { get; private set; }
        public static void Quit()
        {
            _platform.Quit();
        }
    }
}
