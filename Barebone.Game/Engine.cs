using System.Diagnostics;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Game.Physics;
using Barebone.Game.Scene;

namespace Barebone.Game
{
    public class Engine(IPlatform platform)
    {
        public void Run(IActor rootActor)
        {
            const double fixedDeltaT = 1 / 60.0;
            var accumulatedTime = 0.0;

            var graphics = new GraphicsSubSystem(platform.Graphics);
            var input = new InputSubSystem();
            var physics = new PhysicsSubSystem();
            var scene = new SceneSubSystem();
            var clock = new Clock();
            scene.Add(rootActor);

            var bbApi = new BBApi(clock, graphics);

            var timer = Stopwatch.StartNew();
            var timePreviousFrame = -fixedDeltaT; // start with a normal deltaT for the first frame
            var gameTime = 0.0;

            while (!platform.ShouldQuit && !bbApi.QuitRequested)
            {
                platform.ProcessEvents();
                input.Update();

                var time = timer.Elapsed.TotalSeconds;
                var timeElapsed = time - timePreviousFrame;
                accumulatedTime += timeElapsed;

                while (accumulatedTime >= fixedDeltaT)
                {
                    gameTime += fixedDeltaT;
                    clock.StartNextFrame((float)gameTime, (float)fixedDeltaT);
                    physics.Step(fixedDeltaT);
                    scene.Update(bbApi);

                    accumulatedTime -= fixedDeltaT;
                }

                graphics.BeginFrame();
                rootActor.Draw(bbApi);
                graphics.EndFrame();

                platform.Present();
                timePreviousFrame = time;
            }
        }
    }
}
