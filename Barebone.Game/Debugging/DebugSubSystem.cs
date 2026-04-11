using System.Diagnostics;
using Barebone.Game.Input;

namespace Barebone.Game.Debugging
{
    internal class DebugSubSystem(Engine engine) : IDebug
    {
        public void Update(IBBApi bb)
        {
            if (bb.Input.JustPressed(KeyboardKey.Grave)) WriteDebugScreen();
            if (bb.Input.JustPressed(KeyboardKey.NumPadPlus)) { IncreaseGameSpeed(); WriteDebugScreen(); } 
            if (bb.Input.JustPressed(KeyboardKey.NumPadMinus)) { DecreaseGameSpeed(); WriteDebugScreen(); }
            if (bb.Input.JustPressed(KeyboardKey.NumPadEnter)) { WriteDebugScreen(); Debugger.Break(); }

            if (engine.UpdateTime > 0.016f)
                WriteLine($"Frame {bb.Clock.FrameNumber}: UPDATE IS SLOW: {engine.UpdateTime*1000:0.0}ms");
            if (engine.DrawTime > 0.016f)
                WriteLine($"DRAW IS SLOW: {engine.DrawTime * 1000:0.0}ms");
        }
        
        public void IncreaseGameSpeed()
        {
            engine.Speed *= 2f;
            
        }

        public void DecreaseGameSpeed()
        {
            engine.Speed *= 0.5f;
        }

        public float GameSpeed => engine.Speed;

        private void WriteDebugScreen()
        {
            Clear();
            WriteLine("BareBoneGame - DEBUG MENU");
            WriteLine("-------------------------");
            WriteLine($"[+] [-]  Speed: {engine.Speed*100:0}%");
            WriteLine( "[Enter]  Debug break");
            WriteLine();
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void WriteLine(object? msg = null)
        {
            Console.WriteLine(msg?.ToString());
        }

        public void Write(object msg)
        {
            Console.Write(msg.ToString());
        }
    }
}
