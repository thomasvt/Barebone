using System.Drawing;
using Barebone.Graphics.Gpu;
using Barebone.Graphics.NodeArt.Core;
using BareBone.Geometry.Triangulation;
using System.Numerics;
using BareBone.Graphics;
using Point = Barebone.Graphics.NodeArt.Core.Point;

namespace Barebone.Graphics.NodeArt.Drawers
{
    public class NodeArtTesselator
    {
        private readonly Triangulator _triangulator = new();
        private readonly BBList<Vector2> _polygonBuffer = new();

        /// <summary>
        /// Tesselates the given geometry into triangles for GPU rendering and appends them to your triangle buffer.
        /// </summary>
        public void Tesselate(in GeometrySet geometry, in BBList<GpuTexTriangle> triangleBuffer)
        {
            var points = geometry.PointSet.Items;

            var fillColors = geometry.ShapeSet.GetAttributeArrayOrNull<Color>("color");
            var strokeColors = geometry.PathSet.GetAttributeArrayOrNull<Color>("color");
            var strokeThickness = geometry.PathSet.GetAttributeArrayOrNull<float>("thickness");

            foreach (var shape in geometry.ShapeSet.Items)
            {
                TesselateShape(_polygonBuffer, geometry, shape, points);

                if (fillColors != null)
                {
                    var color = fillColors.Get(shape.Idx);
                    if (color.A > 0)
                        FillPolygon(triangleBuffer, _polygonBuffer, color.ToGpuColor());
                }

            }

            //foreach (var segment in geometry.SegmentSet.Segments.AsReadOnlySpan())
            //{
            //    var p0 = points[segment.PointIdx0];
            //    var p1 = points[segment.PointIdx1];
            //    DrawSegment(buffer, p0, p1);
            //}
        }

        private void FillPolygon(in BBList<GpuTexTriangle> triangleBuffer, in BBList<Vector2> corners, in GpuColor color)
        {
            var cornerSpan = corners.AsReadOnlySpan();
            foreach (var triangle in _triangulator.Triangulate(_polygonBuffer.AsArraySegment()))
            {
                var a = new GpuTexVertex(new Vector3(cornerSpan[triangle.A], 0), color, Vector2.Zero);
                var b = new GpuTexVertex(new Vector3(cornerSpan[triangle.B], 0), color, Vector2.Zero);
                var c = new GpuTexVertex(new Vector3(cornerSpan[triangle.C], 0), color, Vector2.Zero);

                triangleBuffer.Add(new GpuTexTriangle(a, b, c));
            }
        }

        private static void TesselateShape(BBList<Vector2> buffer, GeometrySet geometry, Shape shape, ReadOnlySpan<Point> points)
        {
            buffer.Clear();
            var paths = geometry.PathSet.Items;
            var path = paths[shape.PathIdx];
            var segments = geometry.SegmentSet.Items;
            for (var segmentIdx = path.FirstSegmentIdx; segmentIdx <= path.LastSegmentIdx; segmentIdx++)
            {
                var segment = segments[segmentIdx];
                buffer.Add(points[segment.PointIdx0].Position);
            }
        }
    }
}
