using System.Diagnostics;
using Barebone.Game.Debug;
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
            var virtualTimeAccu = 0.0;

            var camera = new Camera();
            var draw = new DrawSubSystem(platform.Graphics, camera);
            var input = new InputSubSystem();
            var physics = new PhysicsSubSystem();
            var scene = new SceneSubSystem();
            var clock = new Clock();
#if DEBUG
            var debug = new DebugSubSystem(this);
#endif
            scene.Add(rootActor);

            var bbApi = new BBApi(clock, draw, camera, input, debug);

            var timer = Stopwatch.StartNew();
            var realTimePreviousFrame = -fixedDeltaT; // start with a normal deltaT for the first frame
            var gameTime = 0.0;

            while (!platform.IsQuitRequested && !bbApi.QuitRequested)
            {
                var realTime = timer.Elapsed.TotalSeconds;
                var realTimeElapsed = realTime - realTimePreviousFrame;
                var virtualTimeElapsed = realTimeElapsed * Speed;
                virtualTimeAccu += virtualTimeElapsed;

                platform.ProcessEvents(input);

                camera.SetViewportSize(platform.GetWindowSize()); // calculate transforms etc, after processing OS events that may have altered the window

                while (virtualTimeAccu >= fixedDeltaT)
                {
                    gameTime += fixedDeltaT;
                    clock.BeginFrame((float)gameTime, (float)fixedDeltaT);
                    physics.Update(fixedDeltaT);

                    scene.Update(bbApi);
#if DEBUG
                    debug.Update(bbApi);
#endif

                    input.EndFrame();
                    virtualTimeAccu -= fixedDeltaT;
                }

                draw.BeginFrame();
                rootActor.Draw(bbApi);
                draw.EndFrame();

                platform.Present();
                realTimePreviousFrame = realTime;
            }
        }

        public float Speed { get; set; } = 1f;
    }
}
