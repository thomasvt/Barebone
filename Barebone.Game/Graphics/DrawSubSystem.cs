using System.Drawing;
using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Game.Graphics
{
    internal enum DrawCommandType
    {
        Clear,
        FillPolygon,
    }

    internal record struct DrawCommand(DrawCommandType Type, in Polygon8 Polygon, in Color Color);

    internal class DrawSubSystem(IPlatformGraphics pg) : IDraw
    {
        private readonly List<DrawCommand> _commands = new();

        public void BeginFrame()
        {
           
        }

        public void ClearScreen(in Color color)
        {
            _commands.Add(new(DrawCommandType.Clear, default, color));
        }

        public void FillAabb(in Aabb box, in Color color)
        {
            _commands.Add(new(DrawCommandType.FillPolygon, Polygon8.FromAabb(box), color));
        }

        public void FillPolygon(in Polygon8 polygon, in Color color)
        {
            _commands.Add(new(DrawCommandType.FillPolygon, polygon, color));
        }

        public void EndFrame()
        {
            foreach (var command in _commands)
            {
                switch (command.Type)
                {
                    case DrawCommandType.Clear: pg.ClearScreen(command.Color); break;
                    case DrawCommandType.FillPolygon: pg.FillPolygon(command.Polygon, command.Color); break;
                }
            }
            _commands.Clear();
        }
    }
}
