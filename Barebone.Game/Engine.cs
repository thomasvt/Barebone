using System.Diagnostics;
using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;

namespace Barebone.Game
{
    public class Engine : IDisposable
    {
        private readonly IPlatform _platform;
        private readonly Camera _camera;
        private readonly DrawSubSystem _draw;
        private readonly InputSubSystem _input;
        private readonly PhysicsSubSystem _physics;
        // private readonly SceneSubSystem _scene;
        private readonly Clock _clock;
        private readonly DebugSubSystem _debug;

        public Engine(IPlatform platform)
        {
            _platform = platform;

            _camera = new Camera();
            _draw = new DrawSubSystem(platform.Graphics, _camera);
            _input = new InputSubSystem();
            _physics = new PhysicsSubSystem();
            // _scene = new SceneSubSystem(_physics);
            _clock = new Clock();

#if DEBUG
            _debug = new DebugSubSystem(this);
#endif
            BB.Init(_clock, _draw, _camera, _input, _debug, _physics);
        }

        public void Run(Func<IGame> gameFactory)
        {
            const double fixedDeltaT = 1 / 60.0;
            var virtualTimeAccu = 0.0;

            var timer = Stopwatch.StartNew();
            var realTimePreviousFrame = -fixedDeltaT; // start with a normal deltaT for the first frame
            var gameTime = 0.0;

            var game = gameFactory.Invoke();

            while (!_platform.IsQuitRequested && !BB.QuitRequested)
            {
                var realTime = timer.Elapsed.TotalSeconds;
                var realTimeElapsed = realTime - realTimePreviousFrame;
                var virtualTimeElapsed = realTimeElapsed * Speed;
                virtualTimeAccu += virtualTimeElapsed;

                _platform.ProcessEvents(_input);

                _camera.SetViewportSize(_platform.GetWindowSize()); // calculate transforms etc, after processing OS events that may have altered the window

                while (virtualTimeAccu >= fixedDeltaT)
                {
                    gameTime += fixedDeltaT;
                    _clock.BeginFrame((float)gameTime, (float)fixedDeltaT);
                    _physics.Update(fixedDeltaT, 4);

                    var swUpdate = Stopwatch.StartNew();
                    game.Update();
                    UpdateTime = swUpdate.Elapsed.TotalSeconds;
#if DEBUG
                    _debug.Update();
#endif

                    _input.EndFrame();
                    if (UpdateTime > fixedDeltaT && virtualTimeAccu > fixedDeltaT * 3)
                        virtualTimeAccu = 0f; // if performance is problematic, we swallow update-frames to not escallate iteration-count of this inner while loop..
                    else
                        virtualTimeAccu -= fixedDeltaT;
                }

                var swDraw = Stopwatch.StartNew();
                _draw.BeginFrame();
                game.Draw();
                _draw.EndFrame();
                DrawTime = swDraw.Elapsed.TotalSeconds;

                _platform.Present();
                realTimePreviousFrame = realTime;
            }
        }

        public float Speed { get; set; } = 1f;
        public double UpdateTime { get; set; }
        public double DrawTime { get; set; }

        public void Dispose()
        {
            _physics.Dispose();
        }
    }
}
