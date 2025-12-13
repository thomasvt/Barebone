using Barebone.Assets;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Input;
using Microsoft.Xna.Framework;

namespace Barebone.Monogame
{
    public class Settings
    {
        public string WindowTitle;
        public bool IsFullScreen;
        public Vector2I Size;
        public bool AllowWindowResizing;
        public bool IsMouseVisible;
        public bool AllowHardwareModeSwitch;
    }

    /// <summary>
    /// Root system for running a <see cref="IGame"/> on Monogame.
    /// </summary>
    public class XnaEngine : Game, IPlatform
    {
        private readonly Func<IPlatform, IGame> _gameFactory;
        private IGame? _game;
        private readonly GraphicsDeviceManager _gdm;
        private readonly XnaInput _input;
        private XnaImmediateRenderer? _renderer;
        private XnaTextureLoader _textureLoader;

        public XnaEngine(Func<IPlatform, IGame> gameFactory, Settings? settings = null)
        {
            _gameFactory = gameFactory;
            _gdm = new GraphicsDeviceManager(this)
            {
                HardwareModeSwitch = settings?.AllowHardwareModeSwitch ?? false,
                IsFullScreen = settings?.IsFullScreen ?? false,
                PreferredBackBufferWidth = settings?.Size.X ?? 800,
                PreferredBackBufferHeight = settings?.Size.Y ?? 600
            };

            Window.Title = settings?.WindowTitle ?? "Barebone";
            IsMouseVisible = settings?.IsMouseVisible ?? true;
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 60.0);
            IsFixedTimeStep = true; // Execute() is called 60 times per game-second, disregarding render framerate, also important for box2d physics.
            Window.AllowUserResizing = settings?.AllowWindowResizing ?? false;
            Window.ClientSizeChanged += WindowOnClientSizeChanged;
            Activated += (sender, args) => WindowActiveChanged?.Invoke(true);
            Deactivated += (sender, args) => WindowActiveChanged?.Invoke(false);

            _input = new XnaInput(Window);
        }

        public event Action<bool>? WindowActiveChanged;

        private void XnaEngine_Deactivated(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

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
            _game!.Dispose();
            _renderer!.Dispose();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();
            _game!.Update((float)gameTime.TotalGameTime.TotalSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void Draw(GameTime gameTime)
        {
            _game!.Draw((float)gameTime.TotalGameTime.TotalSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public IInput Input => _input;
        public ITextureLoader TextureLoader => _textureLoader ?? throw new Exception("TextureLoader not initialized yet.");
        public IImmediateRenderer Renderer => _renderer ?? throw new Exception("Renderer not initialized yet.");
    }
}
