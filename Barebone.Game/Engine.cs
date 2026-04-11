using System.Diagnostics;
using Barebone.Game.Debugging;
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

                    var swUpdate = Stopwatch.StartNew();
                    scene.Update(bbApi);
                    UpdateTime = swUpdate.Elapsed.TotalSeconds;
#if DEBUG
                    debug.Update(bbApi);
#endif

                    input.EndFrame();
                    if (UpdateTime > fixedDeltaT && virtualTimeAccu > fixedDeltaT * 3)
                        virtualTimeAccu = 0f; // if performance is problematic, we swallow update-frames to not escallate iteration-count of this inner while loop..
                    else
                        virtualTimeAccu -= fixedDeltaT;
                }

                var swDraw = Stopwatch.StartNew();
                draw.BeginFrame();
                rootActor.Draw(bbApi);
                draw.EndFrame();
                DrawTime = swDraw.Elapsed.TotalSeconds;

                platform.Present();
                realTimePreviousFrame = realTime;
            }
        }

        public float Speed { get; set; } = 1f;
        public double UpdateTime { get; set; }
        public double DrawTime { get; set; }
    }
}
