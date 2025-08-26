using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using BareBone.Graphics;
using Barebone.Pools;

namespace Barebone.Graphics
{
    public class DebugDraw(float strokeWidthBase) : IDisposable
    {
        private readonly static Color DefaultColor = ColorExt.Parse("#FF7FFF66");
        private record struct DebugDrawItem(Polygon8 Polygon, Color? FillColor, Color? StrokeColor, int FramesToLive, float StrokeWidth);

        private readonly BBList<DebugDrawItem> _items = Pool.Rent<BBList<DebugDrawItem>>();
        private readonly Mesh _mesh = Pool.Rent<Mesh>();

        public void DrawPolygon(Polygon8 polygon, float strokeWidth = 1f, int framesToLive = 30)
        {
            DrawPolygon(polygon, null, DefaultColor, strokeWidth, framesToLive);
        }

        public void DrawPolygon(Polygon8 polygon, Color? fillColor, Color? strokeColor, float strokeWidth = 1f, int framesToLive = 30)
        {
            _items.Add(new(polygon, fillColor, strokeColor, framesToLive, strokeWidth));
        }

        public void RenderAll(IImmediateRenderer renderer)
        {
            var span = _items.AsSpan();
            for (var i = _items.Count-1; i >= 0; i--)
            {
                ref var item = ref span[i];
                if (item.FramesToLive <= 0)
                    _items.SwapRemoveAt(i);
                item.FramesToLive--;
            }

            _mesh.Clear();
            foreach (var item in _items.AsReadOnlySpan())
            {
                if (item.FillColor.HasValue)
                    _mesh.FillPolygonInZ(item.Polygon, 0f, item.FillColor.Value);
                if (item is { StrokeColor: not null, StrokeWidth: > 0f })
                    _mesh.DrawPolygonInZ(item.Polygon, item.StrokeWidth * strokeWidthBase, 0, item.StrokeColor.Value);
            }
            renderer.Draw(Matrix4x4.Identity, _mesh);
        }

        public void Dispose()
        {
            Pool.Return(_items);
            Pool.Return(_mesh);
        }
    }
}
