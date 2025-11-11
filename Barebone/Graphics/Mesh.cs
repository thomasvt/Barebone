using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Geometry.Triangulation;
using BareBone.Graphics;
using Barebone.Pools;
using Barebone.Graphics.Gpu;

namespace Barebone.Graphics
{
    /// <summary>
    /// Renderable Mesh, with procedural drawing methods. Not threadsafe!
    /// </summary>
    public class Mesh : Poolable
    {
        public BBList<GpuTriangle> Triangles = null!;

        protected internal override void Construct()
        {
            Triangles = Pool.Rent<BBList<GpuTriangle>>();
        }

        protected internal override void Destruct()
        {
            Pool.Return(Triangles!);
            Triangles = null!;
        }

        public void Clear()
        {
            Triangles.Clear();
        }

        public Mesh FillTriangle(in GpuVertex a, in GpuVertex b, in GpuVertex c)
        {
            Triangles.Add(new GpuTriangle(a, b, c));
            return this;
        }

        public Mesh FillTriangle(in Vector3 a, in Vector3 b, in Vector3 c, in Color color)
        {
            var gpuColor = color.ToGpuColor();
            Triangles.Add(new GpuTriangle(new(a, gpuColor), new(b, gpuColor), new(c, gpuColor)));
            return this;
        }

        public Mesh FillTriangle(in Triangle3 t, in Color color)
        {
           return FillTriangle(t.A, t.B, t.C, color);
        }

        public Mesh FillTriangleInZ(in Triangle2 t, in float z, in Color color)
        {
            return FillTriangle(t.A.ToVector3(z), t.B.ToVector3(z), t.C.ToVector3(z), color);
        }

        public Mesh FillQuad(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, in Color color)
        {
            var gpuColor = color.ToGpuColor();
            var aGpu = new GpuVertex(a, gpuColor);
            var bGpu = new GpuVertex(b, gpuColor);
            var cGpu = new GpuVertex(c, gpuColor);
            var dGpu = new GpuVertex(d, gpuColor);
            Triangles.Add(new GpuTriangle(aGpu, bGpu, cGpu));
            Triangles.Add(new GpuTriangle(aGpu, cGpu, dGpu));
            return this;
        }

        public Mesh FillQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float z, in Color color)
        {
            var gpuColor = color.ToGpuColor();
            var aGpu = new GpuVertex(a.ToVector3(z), gpuColor);
            var bGpu = new GpuVertex(b.ToVector3(z), gpuColor);
            var cGpu = new GpuVertex(c.ToVector3(z), gpuColor);
            var dGpu = new GpuVertex(d.ToVector3(z), gpuColor);
            Triangles.Add(new GpuTriangle(aGpu, bGpu, cGpu));
            Triangles.Add(new GpuTriangle(aGpu, cGpu, dGpu));
            return this;
        }

        public Mesh FillAabbInZ(in Aabb aabb, in float z, in Color color)
        {
            return FillQuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, z, color);
        }

        public Mesh StrokeQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float halfWidth, in float z, in Color color)
        {
            LineInZ(a, b, halfWidth, z, color);
            LineInZ(b, c, halfWidth, z, color);
            LineInZ(c, d, halfWidth, z, color);
            LineInZ(d, a, halfWidth, z, color);
            return this;
        }

        public Mesh StrokeAabbInZ(in Aabb aabb, in float halfWidth, in float z, in Color color)
        {
            return StrokeQuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, halfWidth, z, color);
        }

        public unsafe Mesh FillPolygonInZ(in Polygon8 polygon, in float z, in Color color)
        {
            if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

            Span<Triangle2> buffer = stackalloc Triangle2[TriangulatorConvex.Shared.GetTriangleCount(polygon.Count)];
            TriangulatorConvex.Shared.Triangulate(polygon.AsReadOnlySpan(), buffer);
            foreach (var triangle in buffer)
            {
                FillTriangle(triangle.ToTriangle3(z), color);
            }
            return this;
        }

        public Mesh StrokePolygonInZ(in Polygon8 polygon, float strokeWidth, in float z, in Color color)
        {
            if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

            var span = polygon.AsReadOnlySpan();
            var p0 = span[^1];
            foreach (var p1 in span)
            {
                LineInZ(p0, p1, strokeWidth, z, color);
                p0 = p1;
            }
            return this;
        }

        public Mesh PointInZ(in Vector2 position, in float halfSize, in float z, in Color color)
        {
            return FillRegularPolyInZ(position, halfSize, 4, z, color, color);
        }

        public Mesh FillRegularPolyInZ(in Vector2 center, in float radius, in int segmentCount, in float z, in Color colorIn, in Color colorOut, in float angleOffset = 0f)
        {
            var angleStep = Angles._360 / segmentCount;
            
            var p0 = new Vector2(MathF.Cos(angleOffset), MathF.Sin(angleOffset)) * radius;

            var c = new GpuVertex(center.ToVector3(z), colorIn.ToGpuColor());
            var colorOutGpu = colorOut.ToGpuColor();
            for (var i = 1; i <= segmentCount; i++)
            {
                var a1 = angleOffset + i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                FillTriangle(c, new GpuVertex((center + p1).ToVector3(z), colorOutGpu), new GpuVertex((center + p0).ToVector3(z), colorOutGpu));

                p0 = p1;
            }
            return this;
        }

        public Mesh StrokeRegularPolyInZ(in Vector2 center, in float radius, in float strokeWidth, in int segmentCount, in float z, in Color color, in float angleOffset = 0f)
        {
            var angleStep = Angles._360 / segmentCount;
            var p0 = new Vector2(MathF.Cos(angleOffset), MathF.Sin(angleOffset)) * radius;
            for (var i = 1; i <= segmentCount; i++)
            {
                var a1 = angleOffset + i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                LineInZ(center + p0, center + p1, strokeWidth, z, color);

                p0 = p1;
            }
            return this;
        }

        public Mesh LineInZ(Vector2 a, Vector2 b, float halfWidth, float z, Color color)
        {
            var abNorm = (b - a).NormalizeOrZero();
            var longWidth = abNorm * halfWidth;
            var latWidth = longWidth.CrossLeft();

            return FillQuadInZ(a - longWidth + latWidth, b + longWidth + latWidth, b + longWidth - latWidth, a - longWidth - latWidth, z, color);
        }
    }
}
