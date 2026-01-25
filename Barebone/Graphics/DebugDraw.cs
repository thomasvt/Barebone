using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using BareBone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.Pools;
using Barebone.UI.Text;

namespace Barebone.Graphics
{
    public class DebugDraw(float strokeWidthBase) : IDisposable
    {
        private readonly static Color DefaultColor = ColorExt.Parse("#FF7FFF66");
        private record struct DebugDrawItem(Polygon8 Polygon, Color? FillColor, Color? StrokeColor, int FramesToLive, float StrokeWidth);
        private record struct DebugTextItem(Vector2 Position, string Text, Color? Color, int FramesToLive);

        private readonly BBList<DebugDrawItem> _items = Pool.Rent<BBList<DebugDrawItem>>();
        private readonly BBList<DebugTextItem> _textItems = Pool.Rent<BBList<DebugTextItem>>();
        private readonly ColorMesh _mesh = Pool.Rent<ColorMesh>();
        private readonly BBList<GpuTexTriangle> _texTriangles = Pool.Rent<BBList<GpuTexTriangle>>();
        private Font? _font;

        public void DrawPolygon(Polygon8 polygon, float strokeWidth = 1f, int framesToLive = 1)
        {
            DrawPolygon(polygon, null, DefaultColor, strokeWidth, framesToLive);
        }

        public void DrawPolygon(Polygon8 polygon, Color? fillColor = null, Color? strokeColor = null, float strokeWidth = 1f, int framesToLive = 1)
        {
            _items.Add(new(polygon, fillColor, strokeColor, framesToLive, strokeWidth));
        }

        public void DrawPoint(Vector2 point, float size = 1, int framesToLive = 1)
        {
            DrawPoint(point, DefaultColor, size, framesToLive);
        }

        public void DrawPoint(Vector2 point, Color? color = null, float size = 1f, int framesToLive = 1)
        {
            _items.Add(new(Polygon8.Square(strokeWidthBase * size).Translate(point), color, null, framesToLive, 1f));
        }

        public void DrawText(Vector2 position, string text, Color? color = null, int framesToLive = 1)
        {
            if (_font == null)
                throw new Exception("Call DebugDraw.SetFont(x) before drawing text.");
            _textItems.Add(new(position, text, color, framesToLive));
        }

        public void RenderAll(IImmediateRenderer renderer)
        {
            RenderPolygons(renderer);
            RenderTexts(renderer);
        }

        public void Update()
        {
            var span = _items.AsSpan();
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                ref var item = ref span[i];
                item.FramesToLive--;
                if (item.FramesToLive <= 0)
                    _items.SwapRemoveRange(i);
            }

            var textItemSpan = _textItems.AsSpan();
            for (var i = _textItems.Count - 1; i >= 0; i--)
            {
                ref var item = ref textItemSpan[i];
                item.FramesToLive--;
                if (item.FramesToLive <= 0)
                    _textItems.SwapRemoveRange(i);
            }
        }

        private void RenderPolygons(IImmediateRenderer renderer)
        {
            _mesh.Clear();
            foreach (var item in _items.AsReadOnlySpan())
            {
                if (item.FillColor.HasValue)
                    _mesh.FillPolygonConvexInZ(item.Polygon, 0f, item.FillColor.Value);
                if (item is { StrokeColor: not null, StrokeWidth: > 0f })
                    _mesh.StrokePolygonInZ(item.Polygon, item.StrokeWidth * strokeWidthBase, 0, item.StrokeColor.Value);
            }
            renderer.Draw(Matrix4x4.Identity, _mesh);
        }

        private void RenderTexts(IImmediateRenderer renderer)
        {
            _texTriangles.Clear();
            foreach (var item in _textItems.AsReadOnlySpan())
            {
                _font!.AppendString(false, _texTriangles, item.Text, item.Color ?? DefaultColor, item.Position);
            }
            renderer.Draw(Matrix4x4.Identity, _texTriangles.AsReadOnlySpan(), _font!.Texture);
        }

        public void SetFont(Font font)
        {
            _font = font;
        }

        public void Dispose()
        {
            _texTriangles.Return();
            _items.Return();
            _textItems.Return();
            _mesh.Return();
        }
    }
}
