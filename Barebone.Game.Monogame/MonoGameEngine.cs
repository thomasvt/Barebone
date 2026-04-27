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
        private KeyboardState _keyboardState, _keyboardStatePrevious;
        private MouseState _mouseState, _mouseStatePrevious;

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
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 60f);
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
            if (!IsActive) return;

            EmitKeyboardEvents(input);
            EmitMouseEvents(input);
        }

        private void EmitMouseEvents(InputSubSystem input)
        {
            _mouseStatePrevious = _mouseState;
            _mouseState = Mouse.GetState();

            var position = new Vector2(_mouseState.X, _mouseState.Y);
            if (_mouseState.X < 0 || _mouseState.X >= Window.ClientBounds.Width ||
                _mouseState.Y < 0 || _mouseState.Y >= Window.ClientBounds.Height) // mouse position is relative to window!
                return; // mouse is out of window.

            if (_mouseState.X != _mouseStatePrevious.X || _mouseState.Y != _mouseStatePrevious.Y)
                input.MouseMove(position);

            EmitMouseTransition(input, MouseButton.Left, _mouseStatePrevious.LeftButton, _mouseState.LeftButton, position);
            EmitMouseTransition(input, MouseButton.Right, _mouseStatePrevious.RightButton, _mouseState.RightButton, position);
            EmitMouseTransition(input, MouseButton.Middle, _mouseStatePrevious.MiddleButton, _mouseState.MiddleButton, position);
            EmitMouseTransition(input, MouseButton.X1, _mouseStatePrevious.XButton1, _mouseState.XButton1, position);
            EmitMouseTransition(input, MouseButton.X2, _mouseStatePrevious.XButton2, _mouseState.XButton2, position);
        }

        private void EmitKeyboardEvents(InputSubSystem input)
        {
            _keyboardStatePrevious = _keyboardState;
            _keyboardState = Keyboard.GetState();

            var pressedNow = _keyboardState.GetPressedKeys();
            var pressedBefore = _keyboardStatePrevious.GetPressedKeys();
            foreach (var k in pressedNow)
            {
                if (!_keyboardStatePrevious.IsKeyDown(k)) input.KeyboardDown((KeyboardKey)(int)k);
            }
            foreach (var k in pressedBefore)
            {
                if (!_keyboardState.IsKeyDown(k)) input.KeyboardUp((KeyboardKey)(int)k);
            }
        }

        private static void EmitMouseTransition(InputSubSystem input, MouseButton button, ButtonState previous, ButtonState now, Vector2 pos)
        {
            if (previous == ButtonState.Released && now == ButtonState.Pressed) input.MouseDown(button, pos);
            else if (previous == ButtonState.Pressed && now == ButtonState.Released) input.MouseUp(button, pos);
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

            _engine.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (_engine.IsQuitRequested)
            {
                IsQuitRequested = true;
                Exit();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_engine == null || _graphics == null) return;

            _engine.DrawAll();

            _graphics.RenderFrame();
        }
    }
}
