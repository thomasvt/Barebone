using System.Drawing;
using System.Numerics;
using BareBone.Graphics;
using Barebone.Graphics.Gpu;

namespace Barebone.Graphics.Manifold.Core
{
    public class DebugDrawer
    {
        /// <summary>
        /// Tesselates the given geometry into triangles for GPU rendering and adds them to your triangleBuffer.
        /// </summary>
        public void Draw(in BBList<GpuTexTriangle> buffer, in Geometry geometry)
        {
            foreach (var p in geometry.Points.Points.AsReadOnlySpan())
            {
                DrawDebugPoint(buffer, p);
            }
        }

        private void DrawDebugPoint(in BBList<GpuTexTriangle> triangleBuffer, in Point p)
        {
            var a = new GpuTexVertex(new Vector3(p.Position + new Vector2(-PointHalfSize, -PointHalfSize), 0), PointColor, Vector2.Zero);
            var b = new GpuTexVertex(new Vector3(p.Position + new Vector2(-PointHalfSize, PointHalfSize), 0), PointColor, Vector2.Zero);
            var c = new GpuTexVertex(new Vector3(p.Position + new Vector2(PointHalfSize, PointHalfSize), 0), PointColor, Vector2.Zero);
            var d = new GpuTexVertex(new Vector3(p.Position + new Vector2(PointHalfSize, -PointHalfSize), 0), PointColor, Vector2.Zero);

            triangleBuffer.Add(new(a, b, c));
            triangleBuffer.Add(new(a, c, d));
        }

        public GpuColor PointColor { get; set; } = Color.BurlyWood.ToGpuColor();
        public float PointHalfSize { get; set; } = 0.5f;
    }
}
