using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = System.Numerics.Vector2;
using XnaGame = Microsoft.Xna.Framework.Game;

namespace Barebone.Game.Monogame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IPlatform"/>.
    /// Inherits <see cref="XnaGame"/> so MonoGame's loop drives everything (window pump, input pump,
    /// fixed-timestep Update, Draw, Present). The abstract <see cref="Engine"/> is constructed when
    /// the GraphicsDevice is ready (LoadContent), and we feed it frames through Engine.TickUpdate /
    /// Engine.TickDraw from MonoGame's Update/Draw overrides.
    /// </summary>
    public class MonoGameEngine : XnaGame, IPlatform
    {
        private readonly Func<IGame> _gameFactory;
        private readonly GraphicsDeviceManager _gdm;
        private MonoGameGraphics? _graphics;
        private Engine? _engine;

        // Input state for diff-based event emission.
        private KeyboardState _kbCurr, _kbPrev;
        private MouseState _msCurr, _msPrev;

        public MonoGameEngine(Func<IGame> gameFactory, string windowTitle, Vector2I windowSize)
        {
            _gameFactory = gameFactory;
            _gdm = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = windowSize.X,
                PreferredBackBufferHeight = windowSize.Y,
                GraphicsProfile = GraphicsProfile.HiDef, // needed for >2048 textures and float color formats
                PreferMultiSampling = false,             // we do our own SSAA
                SynchronizeWithVerticalRetrace = true,
                PreferHalfPixelOffset = false, // legacy XNA/D3D9 quirk; on means rasterization is shifted by +½ pixel which accumulates across the bloom passes and biases halos toward bottom-right
            };
            Window.Title = windowTitle;
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166_667); // ~60 Hz
        }

        /// <summary>
        /// Convenience static entry point. Constructs the engine, calls Game.Run, and disposes.
        /// </summary>
        public static void Run(in Func<IGame> gameFactory, in string windowTitle, in Vector2I windowSize)
        {
            using var engine = new MonoGameEngine(gameFactory, windowTitle, windowSize);
            engine.Run();
        }

        // ====================================================================================
        // IPlatform
        // ====================================================================================
        public Vector2I GetWindowSize()
        {
            var pp = GraphicsDevice.PresentationParameters;
            return new Vector2I(pp.BackBufferWidth, pp.BackBufferHeight);
        }

        public IPlatformGraphics Graphics => _graphics
            ?? throw new InvalidOperationException("Graphics not yet initialised; LoadContent has not run.");

        public bool IsQuitRequested { get; private set; }

        public void Present()
        {
            // No-op: MonoGame presents the swapchain itself after the Draw override returns.
        }

        public void ProcessEvents(InputSubSystem input)
        {
            // ---- Keyboard ----
            _kbPrev = _kbCurr;
            _kbCurr = Keyboard.GetState();
            // Edge-detect transitions. KeyboardKey values match XnaKeys exactly so we cast directly.
            var pressedNow = _kbCurr.GetPressedKeys();
            var pressedBefore = _kbPrev.GetPressedKeys();
            for (int i = 0; i < pressedNow.Length; i++)
            {
                var k = pressedNow[i];
                if (!_kbPrev.IsKeyDown(k)) input.KeyboardDown((KeyboardKey)(int)k);
            }
            for (int i = 0; i < pressedBefore.Length; i++)
            {
                var k = pressedBefore[i];
                if (!_kbCurr.IsKeyDown(k)) input.KeyboardUp((KeyboardKey)(int)k);
            }

            // ---- Mouse ----
            _msPrev = _msCurr;
            _msCurr = Mouse.GetState();
            var pos = new Vector2(_msCurr.X, _msCurr.Y);

            if (_msCurr.X != _msPrev.X || _msCurr.Y != _msPrev.Y)
                input.MouseMove(pos);

            EmitMouseTransition(input, MouseButton.Left,   _msPrev.LeftButton,   _msCurr.LeftButton,   pos);
            EmitMouseTransition(input, MouseButton.Right,  _msPrev.RightButton,  _msCurr.RightButton,  pos);
            EmitMouseTransition(input, MouseButton.Middle, _msPrev.MiddleButton, _msCurr.MiddleButton, pos);
            EmitMouseTransition(input, MouseButton.X1,     _msPrev.XButton1,     _msCurr.XButton1,     pos);
            EmitMouseTransition(input, MouseButton.X2,     _msPrev.XButton2,     _msCurr.XButton2,     pos);
        }

        private static void EmitMouseTransition(InputSubSystem input, MouseButton button, ButtonState before, ButtonState now, Vector2 pos)
        {
            if (before == ButtonState.Released && now == ButtonState.Pressed) input.MouseDown(button, pos);
            else if (before == ButtonState.Pressed && now == ButtonState.Released) input.MouseUp(button, pos);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _graphics = new MonoGameGraphics(GraphicsDevice);
            _engine = new Engine(this);
            _engine.StartGame(_gameFactory);
        }

        protected override void UnloadContent()
        {
            _engine?.Dispose();
            _engine = null;
            _graphics?.Dispose();
            _graphics = null;
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (_engine == null) return;

            _engine.TickUpdate((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (_engine.IsQuitRequested)
            {
                IsQuitRequested = true;
                Exit();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_engine == null || _graphics == null) return;

            // Game.Draw lets the user enqueue draw calls into MonoGameGraphics' deferred batch...
            _engine.TickDraw();

            // ...and now we replay them through the SSAA scene RT, the bloom mip chain, and the
            // final composite into the backbuffer. MonoGame presents automatically after this returns.
            _graphics.RenderFrame();
        }
    }
}
