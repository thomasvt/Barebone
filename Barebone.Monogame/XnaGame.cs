using Barebone.Assets;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    public class Settings
    {
        public Vector2I WindowSize;
        public bool AllowWindowResizing;
        public bool IsMouseVisible;
    }

    /// <summary>
    /// Startup for running an <see cref="IGame"/> on Monogame.
    /// </summary>
    public class XnaGame : Game, IPlatform
    {
        private readonly Func<IPlatform, IGame> _gameFactory;
        private IGame? _game;
        private readonly GraphicsDeviceManager _gdm;
        private readonly XnaInput _input;
        private XnaImmediateRenderer? _renderer;
        private XnaTextureLoader _textureLoader;

        public XnaGame(Func<IPlatform, IGame> gameFactory, Settings? settings = null)
        {
            _gameFactory = gameFactory;
            _gdm = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = settings?.WindowSize.X ?? 800,
                PreferredBackBufferHeight = settings?.WindowSize.Y ?? 600
            };

            IsMouseVisible = settings?.IsMouseVisible ?? true;
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 60.0);
            IsFixedTimeStep = true; // Execute() is called 60 times per game-second, disregarding render framerate, also important for box2d physics.
            Window.AllowUserResizing = settings?.AllowWindowResizing ?? false;
            Window.ClientSizeChanged += WindowOnClientSizeChanged;

            _input = new XnaInput();
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
            _game!.Update((float)gameTime.TotalGameTime.TotalSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void Draw(GameTime gameTime)
        {
            _game!.Draw((float)gameTime.TotalGameTime.TotalSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public IInput Input => _input;
        public ITextureLoader TextureLoader => _textureLoader ?? throw new Exception("TextureLoader not initialized yet.");
        public IImmediateRenderer ImmediateRenderer => _renderer ?? throw new Exception("ImmediateRenderer not initialized yet.");
    }
}
