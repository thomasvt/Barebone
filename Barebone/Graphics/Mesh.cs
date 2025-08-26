using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Geometry.Triangulation;
using Barebone.Pools;
using Triangle2 = Barebone.Geometry.Triangle2;

namespace Barebone.Graphics
{
    /// <summary>
    /// Renderable Mesh, with procedural drawing methods. Not threadsafe!
    /// </summary>
    public class Mesh : Poolable
    {
        public BBList<Triangle> Triangles = null!;

        private BBList<Triangle2> _triangleBuffer = null!;

        protected internal override void Construct()
        {
            Triangles = Pool.Rent<BBList<Triangle>>();
            _triangleBuffer = Pool.Rent<BBList<Triangle2>>();
        }

        protected internal override void Destruct()
        {
            Pool.Return(_triangleBuffer);
            _triangleBuffer = null!;
            Pool.Return(Triangles!);
            Triangles = null!;
        }

        public void Clear()
        {
            Triangles!.Clear();
        }

        public Mesh FillTriangle(in Vector3 a, in Vector3 b, in Vector3 c, Color color)
        {
            Triangles!.Add(new Triangle(a, b, c, color));
            return this;
        }

        public Mesh FillTriangle(in Triangle3 t, in Color color)
        {
            Triangles!.Add(new Triangle(t.A, t.B, t.C, color));
            return this;
        }

        public Mesh FillTriangleInZ(in Triangle2 t, in float z, in Color color)
        {
            Triangles!.Add(new Triangle(t.A.ToVector3(z), t.B.ToVector3(z), t.C.ToVector3(z), color));
            return this;
        }

        public Mesh FillQuad(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, Color color)
        {
            Triangles!.Add(new Triangle(a, b, c, color));
            Triangles!.Add(new Triangle(a, c, d, color));
            return this;
        }

        public Mesh FillQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float z, Color color)
        {
            Triangles!.Add(new Triangle(a.ToVector3(z), b.ToVector3(z), c.ToVector3(z), color));
            Triangles!.Add(new Triangle(a.ToVector3(z), c.ToVector3(z), d.ToVector3(z), color));
            return this;
        }

        public Mesh FillQuadInZ(in Aabb aabb, in float z, Color color)
        {
            return FillQuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, z, color);
        }

        public Mesh DrawQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float halfWidth, in float z, Color color)
        {
            LineInZ(a, b, halfWidth, z, color);
            LineInZ(b, c, halfWidth, z, color);
            LineInZ(c, d, halfWidth, z, color);
            LineInZ(d, a, halfWidth, z, color);
            return this;
        }

        public Mesh DrawQuadInZ(in Aabb aabb, in float halfWidth, in float z, Color color)
        {
            return DrawQuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, halfWidth, z, color);
        }

        public Mesh FillPolygonInZ(in Polygon8 polygon, in float z, in Color color)
        {
            if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

            TriangulatorConvex.Shared.Triangulate(polygon.AsReadOnlySpan(), _triangleBuffer);
            foreach (var triangle in _triangleBuffer.AsReadOnlySpan())
            {
                FillTriangle(triangle.ToTriangle3(z), color);
            }
            return this;
        }

        public Mesh DrawPolygonInZ(in Polygon8 polygon, float strokeWidth, in float z, in Color color)
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
            return FillCircleInZ(position, halfSize, 4, z, color);
        }

        public Mesh FillCircleInZ(in Vector2 center, in float radius, in int segmentCount, in float z, in Color color, in float angleOffset = 0f)
        {
            var angleStep = Angles._360 / segmentCount;
            
            var p0 = new Vector2(MathF.Cos(angleOffset), MathF.Sin(angleOffset)) * radius;
            for (var i = 1; i <= segmentCount; i++)
            {
                var a1 = angleOffset + i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                FillTriangleInZ(new Triangle2(center, center + p1, center + p0), z, color);

                p0 = p1;
            }
            return this;
        }

        public Mesh DrawCircleInZ(in Vector2 center, in float radius, in float strokeWidth, in int segmentCount, in float z, in Color color, in float angleOffset = 0f)
        {
            var angleStep = Angles._360 / segmentCount;
            var p0 = new Vector2(MathF.Cos(angleOffset), MathF.Sin(angleOffset)) * radius;
            for (var i = 1; i <= segmentCount; i++)
            {
                var a1 = angleOffset + i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                LineInZ(p0, p1, strokeWidth, z, color);

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

        /// <summary>
        /// Scales all triangles in this mesh.
        /// </summary>
        public Mesh Scale(float scale)
        {
            foreach (ref var triangle in Triangles!.AsSpan())
            {
                triangle *= scale;
            }
            return this;
        }

        /// <summary>
        /// Translates all triangles in this mesh.
        /// </summary>
        public Mesh Translate(Vector3 translation)
        {
            foreach (ref var triangle in Triangles!.AsSpan())
            {
                triangle += translation;
            }
            return this;
        }

        public Mesh ChangeColor(Color color)
        {
            foreach (ref var triangle in Triangles!.AsSpan())
            {
                triangle.Color = color;
            }
            return this;
        }
    }
}
