using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Geometry.Triangulation;
using BareBone.Geometry.Triangulation;
using BareBone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.Pools;
using Triangle2 = Barebone.Geometry.Triangle2;

namespace Barebone.Graphics;

/// <summary>
/// Renderable Mesh for 2D procedural drawing methods, but supports 3D too. Color only, no texturing. Not threadsafe!
/// </summary>
public abstract class MeshBase<TMesh, TTriangle, TVertex> : Poolable 
    where TTriangle : struct
    where TVertex : struct
    where TMesh : MeshBase<TMesh, TTriangle, TVertex>
{
    public BBList<TTriangle> Triangles = null!;

    protected internal override void Construct()
    {
        Triangles = Pool.Rent<BBList<TTriangle>>();
    }

    protected internal override void Destruct()
    {
        Pool.Return(Triangles!);
        Triangles = null!;
    }

    public TMesh Clear()
    {
        Triangles.Clear();
        return (TMesh)this;
    }

    protected abstract TTriangle ToTriangle(in TVertex a, in TVertex b, in TVertex c);

    protected abstract TTriangle ToTriangle(Vector3 a, Vector3 b, Vector3 c, GpuColor gpuColor);

    protected abstract TVertex ToVertex(Vector3 a, GpuColor gpuColor);

    public TMesh AddTriangle(in TVertex a, in TVertex b, in TVertex c)
    {
        Triangles.Add(ToTriangle(a,b,c));
        return (TMesh)this;
    }

    public TMesh FillTriangle(in Vector3 a, in Vector3 b, in Vector3 c, in Color color)
    {
        var gpuColor = color.ToGpuColor();
        Triangles.Add(ToTriangle(a, b, c, gpuColor));
        return (TMesh)this;
    }

    public TMesh FillTriangle(in Triangle3 t, in Color color)
    {
        return FillTriangle(t.A, t.B, t.C, color);
    }

    public TMesh FillTriangleInZ(in Triangle2 t, in float z, in Color color)
    {
        return FillTriangle(t.A.ToVector3(z), t.B.ToVector3(z), t.C.ToVector3(z), color);
    }

    public TMesh AddQuad(in TVertex a, in TVertex b, in TVertex c, in TVertex d)
    {
        Triangles.Add(ToTriangle(a, b, c));
        Triangles.Add(ToTriangle(a, c, d));
        return (TMesh)this;
    }

    public TMesh FillQuad(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, in Color color)
    {
        var gpuColor = color.ToGpuColor();
        var aGpu = ToVertex(a, gpuColor);
        var bGpu = ToVertex(b, gpuColor);
        var cGpu = ToVertex(c, gpuColor);
        var dGpu = ToVertex(d, gpuColor);
        Triangles.Add(ToTriangle(aGpu, bGpu, cGpu));
        Triangles.Add(ToTriangle(aGpu, cGpu, dGpu));
        return (TMesh)this;
    }

    public TMesh FillQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float z, in Color color)
    {
        var gpuColor = color.ToGpuColor();
        var aGpu = ToVertex(a.ToVector3(z), gpuColor);
        var bGpu = ToVertex(b.ToVector3(z), gpuColor);
        var cGpu = ToVertex(c.ToVector3(z), gpuColor);
        var dGpu = ToVertex(d.ToVector3(z), gpuColor);
        Triangles.Add(ToTriangle(aGpu, bGpu, cGpu));
        Triangles.Add(ToTriangle(aGpu, cGpu, dGpu));
        return (TMesh)this;
    }

    public TMesh FillAabbInZ(in Aabb aabb, in float z, in Color color)
    {
        return FillQuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, z, color);
    }

    public TMesh StrokeQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float halfWidth, in float z, in Color color)
    {
        LineInZ(a, b, halfWidth, z, color);
        LineInZ(b, c, halfWidth, z, color);
        LineInZ(c, d, halfWidth, z, color);
        LineInZ(d, a, halfWidth, z, color);
        return (TMesh)this;
    }

    public TMesh StrokeAabbInZ(in Aabb aabb, in float halfWidth, in float z, in Color color)
    {
        return StrokeQuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, halfWidth, z, color);
    }

    public unsafe TMesh FillPolygonConvexInZ(in Polygon8 polygon, in float z, in Color color)
    {
        if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

        Span<IndexTriangle> buffer = stackalloc IndexTriangle[TriangulatorConvex.GetTriangleCount(polygon.Count)];
        TriangulatorConvex.Triangulate(polygon.Count, buffer);
        var corners = polygon.AsReadOnlySpan();
        foreach (var t in buffer)
        {
            var a2 = corners[t.A];
            var b2 = corners[t.B];
            var c2 = corners[t.C];

            var a = new Vector3(a2, z);
            var b = new Vector3(b2, z);
            var c = new Vector3(c2, z);

            FillTriangle(a, b, c, color);
        }
        return (TMesh)this;
    }

    public TMesh StrokePolygonInZ(in Polygon8 polygon, float strokeWidth, in float z, in Color color)
    {
        if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

        var span = polygon.AsReadOnlySpan();
        var p0 = span[^1];
        foreach (var p1 in span)
        {
            LineInZ(p0, p1, strokeWidth, z, color);
            p0 = p1;
        }
        return (TMesh)this;
    }

    public TMesh PointInZ(in Vector2 position, in float halfSize, in float z, in Color color)
    {
        return FillCircleInZ(position, halfSize, 4, z, color, color);
    }

    public TMesh FillCircleInZ(in Vector2 center, in float radius, in int segmentCount, in float z, in Color colorIn, in Color? colorOut = null, in float angleOffset = 0f)
    {
        var angleStep = Angles._360 / segmentCount;

        var (sin, cos) = MathF.SinCos(angleOffset);
        var p0 = new Vector2(cos, sin) * radius;

        var c = ToVertex(center.ToVector3(z), colorIn.ToGpuColor());
        var colorOutGpu = (colorOut ?? colorIn).ToGpuColor();
        for (var i = 1; i <= segmentCount; i++)
        {
            var a1 = angleOffset + i * angleStep;
            (sin, cos) = MathF.SinCos(a1);
            var p1 = new Vector2(cos, sin) * radius;

            AddTriangle(c, ToVertex((center + p1).ToVector3(z), colorOutGpu), ToVertex((center + p0).ToVector3(z), colorOutGpu));

            p0 = p1;
        }
        return (TMesh)this;
    }

    public TMesh FillEllipseInZ(in Vector2 center, in Vector2 radius, in int segmentCount, in float z, in Color color)
    {
        var angleStep = Angles._360 / segmentCount;

        var p0 = new Vector2(1, 0) * radius.X;

        var c = ToVertex(center.ToVector3(z), color.ToGpuColor());
        var gpuColor = color.ToGpuColor();
        for (var i = 1; i <= segmentCount; i++)
        {
            var a1 = i * angleStep;
            var cos = MathF.Cos(a1);
            var sin = MathF.Sin(a1);
            var p1 = new Vector2(cos, sin) * radius;

            AddTriangle(c, ToVertex((center + p1).ToVector3(z), gpuColor), ToVertex((center + p0).ToVector3(z), gpuColor));

            p0 = p1;
        }
        return (TMesh)this;
    }

    public TMesh StrokeRegularPolyInZ(in Vector2 center, in float radius, in float strokeWidth, in int segmentCount, in float z, in Color color, in float angleOffset = 0f)
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
        return (TMesh)this;
    }

    public TMesh LineInZ(Vector2 a, Vector2 b, float halfWidth, float z, Color color)
    {
        var abNorm = (b - a).NormalizeOrZero();
        var longWidth = abNorm * halfWidth;
        var latWidth = longWidth.CrossLeft();

        return FillQuadInZ(a - longWidth + latWidth, b + longWidth + latWidth, b + longWidth - latWidth, a - longWidth - latWidth, z, color);
    }
}