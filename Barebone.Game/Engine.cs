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

            var draw = new DrawSubSystem(platform.Graphics);
            var input = new InputSubSystem();
            var physics = new PhysicsSubSystem();
            var scene = new SceneSubSystem();
            var clock = new Clock();
            scene.Add(rootActor);

            var bbApi = new BBApi(clock, draw, input);

            var timer = Stopwatch.StartNew();
            var timePreviousFrame = -fixedDeltaT; // start with a normal deltaT for the first frame
            var gameTime = 0.0;

            while (!platform.IsQuitRequested && !bbApi.QuitRequested)
            {
                var time = timer.Elapsed.TotalSeconds;
                var timeElapsed = time - timePreviousFrame;
                accumulatedTime += timeElapsed;

                platform.ProcessEvents(input);

                while (accumulatedTime >= fixedDeltaT)
                {
                    gameTime += fixedDeltaT;
                    clock.BeginFrame((float)gameTime, (float)fixedDeltaT);
                    physics.Update(fixedDeltaT);

                    scene.Update(bbApi);

                    input.EndFrame();
                    accumulatedTime -= fixedDeltaT;
                }

                draw.BeginFrame();
                rootActor.Draw(bbApi);
                draw.EndFrame();

                platform.Present();
                timePreviousFrame = time;
            }
        }
    }
}
