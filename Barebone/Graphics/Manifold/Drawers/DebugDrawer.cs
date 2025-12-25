using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using BareBone.Geometry.Triangulation;
using BareBone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.Graphics.Manifold.Core;
using Point = Barebone.Graphics.Manifold.Core.Point;

namespace Barebone.Graphics.Manifold.Drawers
{
    /// <summary>
    /// Converts Manifold output to GPU renderable triangles to visualize points, segments and filled shapes. Is supposed to be rendered without depth buffer.
    /// </summary>
    public class DebugDrawer
    {
        private readonly Triangulator _triangulator = new();
        private readonly BBList<Vector2> _cornerBuffer = new();

        /// <summary>
        /// Tesselates the given geometry into triangles for GPU rendering and adds them to your triangleBuffer.
        /// </summary>
        public void Draw(in BBList<GpuTexTriangle> buffer, in Core.Geometry geometry)
        {
            var points = geometry.PointSet.Points.AsReadOnlySpan();

            foreach (var shape in geometry.ShapeSet.Shapes.AsReadOnlySpan())
            {
                AssembleShapeCorners(_cornerBuffer, geometry, shape, points);
                DrawPolygon(buffer, _cornerBuffer);
            }

            foreach (var segment in geometry.SegmentSet.Segments.AsReadOnlySpan())
            {
                var p0 = points[segment.PointIdx0];
                var p1 = points[segment.PointIdx1];
                DrawSegment(buffer, p0, p1);
            }

            foreach (var p in points)
            {
                DrawPoint(buffer, p);
            }
        }

        private void DrawPoint(in BBList<GpuTexTriangle> triangleBuffer, in Point p)
        {
            var a = new GpuTexVertex(new Vector3(p.Position + new Vector2(-PointHalfSize, -PointHalfSize), 0), PointColor, Vector2.Zero);
            var b = new GpuTexVertex(new Vector3(p.Position + new Vector2(-PointHalfSize, PointHalfSize), 0), PointColor, Vector2.Zero);
            var c = new GpuTexVertex(new Vector3(p.Position + new Vector2(PointHalfSize, PointHalfSize), 0), PointColor, Vector2.Zero);
            var d = new GpuTexVertex(new Vector3(p.Position + new Vector2(PointHalfSize, -PointHalfSize), 0), PointColor, Vector2.Zero);

            triangleBuffer.Add(new(a, b, c));
            triangleBuffer.Add(new(a, c, d));
        }

        private void DrawSegment(in BBList<GpuTexTriangle> triangleBuffer, in Point p0, in Point p1)
        {
            if (p0.Position == p1.Position) return;

            var longDist = Vector2.Normalize(p1.Position - p0.Position) * SegmentHalfWidth;
            var latDist = longDist.CrossLeft();

            var a = new GpuTexVertex(new Vector3(p0.Position - longDist - latDist, 0), SegmentColor, Vector2.Zero);
            var b = new GpuTexVertex(new Vector3(p0.Position - longDist + latDist, 0), SegmentColor, Vector2.Zero);
            var c = new GpuTexVertex(new Vector3(p1.Position + longDist + latDist, 0), SegmentColor, Vector2.Zero);
            var d = new GpuTexVertex(new Vector3(p1.Position + longDist - latDist, 0), SegmentColor, Vector2.Zero);

            triangleBuffer.Add(new(a, b, c));
            triangleBuffer.Add(new(a, c, d));
        }

        private void DrawPolygon(in BBList<GpuTexTriangle> triangleBuffer, in BBList<Vector2> corners)
        {
            var cornerSpan = corners.AsReadOnlySpan();
            foreach (var triangle in _triangulator.Triangulate(_cornerBuffer.AsArraySegment()))
            {
                var a = new GpuTexVertex(new Vector3(cornerSpan[triangle.A], 0), ShapeColor, Vector2.Zero);
                var b = new GpuTexVertex(new Vector3(cornerSpan[triangle.B], 0), ShapeColor, Vector2.Zero);
                var c = new GpuTexVertex(new Vector3(cornerSpan[triangle.C], 0), ShapeColor, Vector2.Zero);

                triangleBuffer.Add(new GpuTexTriangle(a, b, c));
            }
        }

        private static void AssembleShapeCorners(BBList<Vector2> buffer, Core.Geometry geometry, Shape shape, ReadOnlySpan<Point> points)
        {
            buffer.Clear();
            var paths = geometry.PathSet.Paths.AsReadOnlySpan();
            var path = paths[shape.PathIdx];
            var segments = geometry.SegmentSet.Segments.AsReadOnlySpan();
            for (var segmentIdx = path.FirstSegmentIdx; segmentIdx <= path.LastSegmentIdx; segmentIdx++)
            {
                var segment = segments[segmentIdx];
                buffer.Add(points[segment.PointIdx0].Position);
            }
        }

        public GpuColor PointColor { get; set; } = PaletteApollo.Blue3.ToGpuColor();
        public GpuColor SegmentColor { get; set; } = PaletteApollo.Blue2.ToGpuColor();
        public GpuColor ShapeColor { get; set; } = PaletteApollo.Blue1.ToGpuColor();
        public float PointHalfSize { get; set; } = 0.5f;
        public float SegmentHalfWidth { get; set; } = 0.2f;
    }
}
