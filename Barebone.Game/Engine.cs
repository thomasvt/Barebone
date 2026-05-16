using System.Diagnostics;
using Barebone.Game.Debugging;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Messaging;

namespace Barebone.Game
{
    /// <summary>
    /// Middle layers of the game: offers high level API to top layer (the game) and converts into subsystem activity (bottom  layer).
    /// </summary>
    public class Engine : IDisposable
    {
        private readonly IPlatform _platform;
        private readonly GraphicsSubSystem _graphics;
        private readonly InputSubSystem _input;
        private readonly PhysicsSubSystem _physics;
        private readonly Clock _appClock;
        private readonly DebugSubSystem _debug;

        private readonly IGame _game;

        public Engine(Func<IGame> gameFactory, IPlatform platform)
        {
            _platform = platform;
            var messageBus = new MessageBus();
            _graphics = new GraphicsSubSystem(platform.Graphics, messageBus, platform.GetWindowSize().Y);
            _input = new InputSubSystem();
            _physics = new PhysicsSubSystem();
            _appClock = new Clock();

#if DEBUG
            _debug = new DebugSubSystem(this);
#endif
            BB.Init(_appClock, _graphics, _input, _debug, _physics, messageBus, platform);

            _game = gameFactory.Invoke();
        }

        public void Update(float appDeltaT)
        {
            _platform.ProcessPlatformEvents(_input);
            _graphics.SetViewportSize(_platform.GetWindowSize());

            var gameDeltaT = appDeltaT * Speed;
            _appClock.BeginFrame(gameDeltaT);

            var sw = Stopwatch.StartNew();
            _game.Update();
            UpdateTime = sw.Elapsed.TotalSeconds;
#if DEBUG
            _debug.Update();
#endif
            _input.EndFrame();
        }

        public void DrawAll()
        {
            if (_game == null) throw new InvalidOperationException("StartGame() must be called first.");
            var sw = Stopwatch.StartNew();
            _game.Draw();
            DrawTime = sw.Elapsed.TotalSeconds;
        }

        /// <summary>
        /// True when either the platform or game logic asked to quit. External loop drivers should
        /// poll this each frame and exit their loop accordingly.
        /// </summary>
        // public bool IsQuitRequested => _platform.IsQuitRequested || BB.QuitRequested;

        public float Speed { get; set; } = 1f;
        public double UpdateTime { get; set; }
        public double DrawTime { get; set; }

        public void Dispose()
        {
            _graphics.Dispose();
            _physics.Dispose();
        }
    }
}
