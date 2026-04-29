using System.Diagnostics;
using Barebone.Game.Input;

namespace Barebone.Game.Debugging
{
    internal class DebugSubSystem(Engine engine) : IDebug
    {
        public void Update()
        {
            if (BB.Input.JustPressed(KeyboardKey.OemTilde)) WriteDebugScreen();
            if (BB.Input.JustPressed(KeyboardKey.OemPlus)) { IncreaseGameSpeed(); WriteDebugScreen(); } 
            if (BB.Input.JustPressed(KeyboardKey.OemMinus)) { DecreaseGameSpeed(); WriteDebugScreen(); }
            if (BB.Input.JustPressed(KeyboardKey.Enter)) { WriteDebugScreen(); Debugger.Break(); }

            if (engine.UpdateTime > 0.010f)
                WriteLine($"Frame {BB.Clock.FrameNumber}: UPDATE IS SLOW: {engine.UpdateTime*1000:0.0}ms");
            if (engine.DrawTime > 0.010f)
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
            WriteLine($"Update: {engine.UpdateTime*1000:0.00} ms");
            WriteLine($"Draw: {engine.DrawTime * 1000:0.00} ms");
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
