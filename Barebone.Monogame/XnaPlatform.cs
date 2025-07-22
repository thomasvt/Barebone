using Barebone.Graphics;
using Barebone.Inputs;
using Microsoft.Xna.Framework;

namespace Barebone.Monogame
{
    public class XnaPlatform : Game, IPlatform
    {
        private readonly Func<IPlatform, IGame> _gameFactory;
        private IGame? _game;
        private readonly GraphicsDeviceManager _gdm;
        private readonly XnaInput _input;
        private XnaImmediateRenderer? _renderer;

        public XnaPlatform(Func<IPlatform, IGame> gameFactory)
        {
            _gameFactory = gameFactory;
            _gdm = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 800
            };

            IsMouseVisible = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 60.0);
            IsFixedTimeStep = true; // Execute() is called 60 times per game-second, disregarding render framerate, also important for box2d physics.

            _input = new XnaInput();
        }

        public IInput Input => _input;
        public IImmediateRenderer ImmediateRenderer => _renderer ?? throw new Exception("ImmediateRenderer not initialized yet.");
        
        protected override void LoadContent()
        {
            base.LoadContent();
            _renderer = new XnaImmediateRenderer(GraphicsDevice);

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
    }
}
