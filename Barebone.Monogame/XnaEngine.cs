using Barebone.Assets;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Input;
using Microsoft.Xna.Framework;

namespace Barebone.Monogame
{
    public class Settings
    {
        public string WindowTitle = "";
        public bool IsFullScreen;
        public Vector2I WindowSize;
        public bool AllowWindowResizing;
        public bool IsMouseVisible;
        public bool AllowHardwareModeSwitch;
        public double UpdateInterval = 1/60.0;
    }

    /// <summary>
    /// Root system for running a <see cref="IGame"/> on Monogame but with discrete update intervals and VSync rendering with interpolation support for smooth animations.
    /// (https://gafferongames.com/post/fix_your_timestep/)
    /// </summary>
    public class XnaEngine : Game, IPlatform
    {
        private readonly Func<IPlatform, IGame> _gameFactory;
        private IGame? _game;
        private readonly GraphicsDeviceManager _gdm;
        private readonly XnaInput _input;
        private XnaImmediateRenderer? _renderer;
        private XnaTextureLoader _textureLoader = null!;

        private double _time;
        private double _updateInterval;
        private double _updateAccu = 0;

        public XnaEngine(Func<IPlatform, IGame> gameFactory, Settings? settings = null)
        {
            _gameFactory = gameFactory;
            _gdm = new GraphicsDeviceManager(this)
            {
                HardwareModeSwitch = settings?.AllowHardwareModeSwitch ?? false,
                IsFullScreen = settings?.IsFullScreen ?? false,
                PreferredBackBufferWidth = settings?.WindowSize.X ?? 800,
                PreferredBackBufferHeight = settings?.WindowSize.Y ?? 600,
                SynchronizeWithVerticalRetrace = true
            };

            _updateInterval = settings?.UpdateInterval ?? 1/60.0;

            Window.Title = settings?.WindowTitle ?? "Barebone";
            IsMouseVisible = settings?.IsMouseVisible ?? true;
            // TargetElapsedTime = TimeSpan.FromSeconds(1 / 120.0);
            IsFixedTimeStep = false; // fixed updates gives jittery movement visuals due to misaligned update and render times. Interpollation fixes this but it's non-trivial. Unnecessary for simple games.
            Window.AllowUserResizing = settings?.AllowWindowResizing ?? false;
            Window.ClientSizeChanged += WindowOnClientSizeChanged;
            Activated += (sender, args) => WindowActiveChanged?.Invoke(true);
            Deactivated += (sender, args) => WindowActiveChanged?.Invoke(false);

            _input = new XnaInput(Window);
        }

        public event Action<bool>? WindowActiveChanged;
        
        private void WindowOnClientSizeChanged(object? sender, EventArgs e)
        {
            // Get the new window size
            var newSize = new Vector2I(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            _game?.OnWindowSizeChanged(newSize);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            _renderer = new XnaImmediateRenderer(GraphicsDevice, _gdm);
            _textureLoader = new XnaTextureLoader(GraphicsDevice);

            _game = _gameFactory.Invoke(this);
        }

        protected override void UnloadContent()
        {
            (_game as IDisposable)?.Dispose();
            _renderer!.Dispose();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalSeconds;

            dt = Math.Min(dt, 0.25); // prevent explosion by clamping dt to 4Hz.
            _updateAccu += dt;

            // invoke Update() for as many discrete timesteps that fit into the time that has passed.
            while (_updateAccu >= _updateInterval)
            {
                // Sample input once per fixed step Update.
                Input.Update();
                _time += _updateInterval;
                _game!.Update((float)_time, (float)_updateInterval);
                _updateAccu -= _updateInterval;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var remainderFraction = _updateAccu / _updateInterval;
            _game!.Draw((float)remainderFraction, dt, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public IInput Input => _input;
        public ITextureLoader TextureLoader => _textureLoader ?? throw new Exception("TextureLoader not initialized yet.");
        public IImmediateRenderer Renderer => _renderer ?? throw new Exception("Renderer not initialized yet.");
    }
}
