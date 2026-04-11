using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;

namespace Barebone.Game.Graphics
{
    internal enum DrawCommandType
    {
        Clear,
        FillPolygon,
        FillCircle,
    }

    internal record struct DrawCommand(DrawCommandType Type, in Vector2 Position, in float Radius, in int SegmentCount, in Polygon8 Polygon, in Color Color);

    internal class DrawSubSystem(IPlatformGraphics pg, Camera camera) : IDraw
    {
        private readonly List<DrawCommand> _commands = new();

        public void BeginFrame()
        {
        }

        public void ClearScreen(in Color color)
        {
            _commands.Add(new(DrawCommandType.Clear, default, 0, 0, default, color));
        }

        public void FillAabb(in Aabb box, in Color color)
        {
            _commands.Add(new(DrawCommandType.FillPolygon, default, 0, 0, Polygon8.FromAabb(box).Transform(camera.WorldToScreenTransform), color));
        }

        public void Line(in Vector2 a, in Vector2 b, float width, LineCap lineCap, in Color color)
        {
            FillPolygon(Polygon8.Line(a, b, width, lineCap), color);
        }

        public void FillPolygon(in Vector2 position, in Polygon8 polygon, in Color color)
        {
            _commands.Add(new(DrawCommandType.FillPolygon, position, 0, 0, polygon.Transform(camera.WorldToScreenTransform), color));
        }

        public void FillPolygon(in Polygon8 polygon, in Color color)
        {
            _commands.Add(new(DrawCommandType.FillPolygon, Vector2.Zero, 0, 0, polygon.Transform(camera.WorldToScreenTransform), color));
        }

        public void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color)
        {
            center = camera.WorldToScreen(center);
            radius = camera.WorldLengthToScreen(radius);
            _commands.Add(new(DrawCommandType.FillCircle, center, radius, segmentCount, default, color));
        }

        public void EndFrame()
        {
            foreach (var command in _commands)
            {
                switch (command.Type)
                {
                    case DrawCommandType.Clear: pg.ClearScreen(command.Color); break;
                    case DrawCommandType.FillPolygon: FillPolygonInternal(command.Polygon.Translate(command.Position), command.Color); break;
                    case DrawCommandType.FillCircle: FillCircleInternal(command.Position, command.Radius, command.SegmentCount, command.Color); break;
                }
            }
            _commands.Clear();
        }

        private void FillCircleInternal(in Vector2 center, in float radius, in int segmentCount, in Color color)
        {
            var colorF = ColorF.FromColor(color);

            Span<Vertex> vertices = stackalloc Vertex[segmentCount * 3];

            var i = 0;

            var angleStep = Angles._360 / segmentCount;

            var angle = -angleStep;
            var a = center + angle.AngleToVector2(radius);

            for (var s = 0; s < segmentCount; s++)
            {
                angle += angleStep;

                var b = center + angle.AngleToVector2(radius);

                vertices[i++] = new() { Color = colorF, Position = center };
                vertices[i++] = new() { Color = colorF, Position = a };
                vertices[i++] = new() { Color = colorF, Position = b };

                a = b;
            }

            pg.FillTriangles(vertices);
        }

        private void FillPolygonInternal(in Polygon8 polygon, in Color color)
        {
            var colorF = ColorF.FromColor(color);

            var pA = polygon[0];
            Span<Vertex> vertices = stackalloc Vertex[(polygon.Count - 2) * 3];

            var i = 0;
            var pB = polygon[1];

            for (var c = 2; c < polygon.Count; c++)
            {
                var pC = polygon[c]; // Polygon indexer supports wrap-around

                vertices[i++] = new() { Color = colorF, Position = pA };
                vertices[i++] = new() { Color = colorF, Position = pB };
                vertices[i++] = new() { Color = colorF, Position = pC };

                pB = pC;
            }

            pg.FillTriangles(vertices);
        }
    }
}
