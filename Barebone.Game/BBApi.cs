using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Game.Scene;

namespace Barebone.Game
{
    internal class BBApi(IClock clock, IDraw draw, ICamera camera, IInput input, IDebug debug, IScene scene, IPhysics physics) : IBBApi
    {
        public IClock Clock { get; } = clock;

        public IDraw Draw { get; } = draw;
        public ICamera Camera { get; } = camera;
        public IInput Input { get; } = input;
        public IDebug Debug { get; } = debug;
        public IScene Scene { get; } = scene;
        public IPhysics Physics { get; } = physics;

        public void Quit()
        {
            QuitRequested = true;
        }

        public bool QuitRequested { get; private set; }
    }
}
