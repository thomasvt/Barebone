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

        private readonly BBList<Triangle2> _triangleBuffer = new();

        protected internal override void Construct()
        {
            Triangles = Pool.Rent<BBList<Triangle>>();
        }

        protected internal override void Destruct()
        {
            Pool.Return(Triangles!);
            Triangles = null;
        }

        public void Clear()
        {
            Triangles!.Clear();
        }

        public Mesh Triangle(in Vector3 a, in Vector3 b, in Vector3 c, Color color)
        {
            Triangles!.Add(new Triangle(a, b, c, color));
            return this;
        }

        public Mesh Triangle(in Triangle3 t, in Color color)
        {
            Triangles!.Add(new Triangle(t.A, t.B, t.C, color));
            return this;
        }

        public Mesh TriangleInZ(in Triangle2 t, in float z, in Color color)
        {
            Triangles!.Add(new Triangle(t.A.ToVector3(z), t.B.ToVector3(z), t.C.ToVector3(z), color));
            return this;
        }

        public Mesh Quad(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, Color color)
        {
            Triangles!.Add(new Triangle(a, b, c, color));
            Triangles!.Add(new Triangle(a, c, d, color));
            return this;
        }

        public Mesh QuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float z, Color color)
        {
            Triangles!.Add(new Triangle(a.ToVector3(z), b.ToVector3(z), c.ToVector3(z), color));
            Triangles!.Add(new Triangle(a.ToVector3(z), c.ToVector3(z), d.ToVector3(z), color));
            return this;
        }

        public Mesh QuadInZ(in Aabb aabb, in float z, Color color)
        {
            return QuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, z, color);
        }

        public Mesh QuadEdgesInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float halfWidth, in float z, Color color)
        {
            LineInZ(a, b, halfWidth, z, color);
            LineInZ(b, c, halfWidth, z, color);
            LineInZ(c, d, halfWidth, z, color);
            LineInZ(d, a, halfWidth, z, color);
            return this;
        }

        public Mesh QuadEdgesInZ(in Aabb aabb, in float halfWidth, in float z, Color color)
        {
            return QuadEdgesInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, halfWidth, z, color);
        }

        /// <summary>
        /// Draws a convex polygon extruded along a given distance. If 'extrude' is negative, the polygon will appear pushed inwards. 
        /// </summary>
        public Mesh ExtrudedPolygon(in Polygon8 polygon, float z, float extrude, in Color topColor, in Color sideColor)
        {
            return ExtrudedPolygon(polygon.AsReadOnlySpan(), z, extrude, in topColor, in sideColor);
        }

        /// <summary>
        /// Draws a convex polygon extruded along a given distance. If 'extrude' is negative, the polygon will appear pushed inwards. 
        /// </summary>
        public Mesh ExtrudedPolygon(in ReadOnlySpan<Vector2> corners, float z, in float extrude, in Color topColor, in Color sideColor)
        {
            if (corners.Length < 3) throw new Exception("Polygons must have at least 3 corners.");

            TriangulatorConvex.Shared.Triangulate(corners, _triangleBuffer);
            foreach (var triangle in _triangleBuffer.AsReadOnlySpan())
            {
                Triangle(triangle.ToTriangle3(z + extrude), topColor);
            }

            var a = corners[0];
            for (var i = 0; i < corners.Length; i++)
            {
                var b = corners[(i + 1) % corners.Length];
                Quad(new Vector3(a, z), new Vector3(b, z), new Vector3(b, z + extrude), new Vector3(a, z + extrude), sideColor);
                a = b;
            }

            return this;
        }

        public Mesh RegularPolygonInZ(float radius, int sideCount, float z, Color color, float angleOffset = 0f)
        {
            if (sideCount < 3) throw new ArgumentOutOfRangeException(nameof(sideCount), "SideCount for a regular polygon must be at least 3.");

            var angleStep = Angles._360 / sideCount;
            for (var i = 0; i < sideCount; i++)
            {
                var a0 = angleOffset + i * angleStep;
                var a1 = a0 + angleStep;

                var p0 = new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                Triangle(new(0, 0, z), new (p0, z), new(p1, z), color);
            }

            return this;
        }

        public Mesh Polygon(in Polygon8 polygon, in float z, in Color color)
        {
            if (polygon.Count < 3) throw new Exception("Polygons must have at least 3 corners.");

            TriangulatorConvex.Shared.Triangulate(polygon.AsReadOnlySpan(), _triangleBuffer);
            foreach (var triangle in _triangleBuffer.AsReadOnlySpan())
            {
                Triangle(triangle.ToTriangle3(z), color);
            }
            return this;
        }

        public Mesh PointInZ(in Vector2 position, in float halfSize, in float z, in Color color)
        {
            return CircleInZ(position, halfSize, 4, z, color);
        }

        private Mesh CircleInZ(in Vector2 center, in float radius, in int segmentCount, in float z, in Color color)
        {
            var angleStep = Angles._360 / segmentCount;
            var p0 = new Vector2(MathF.Cos(0f), MathF.Sin(0f)) * radius;
            for (var i = 1; i <= segmentCount; i++)
            {
                var a1 = i * angleStep;
                var p1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                TriangleInZ(new Triangle2(center, center + p1, center + p0), z, color);

                p0 = p1;
            }
            return this;
        }

        public Mesh LineInZ(Vector2 a, Vector2 b, float halfWidth, float z, Color color)
        {
            var abNorm = (b - a).NormalizeOrZero();
            var longWidth = abNorm * halfWidth;
            var latWidth = longWidth.CrossLeft();

            return QuadInZ(a - longWidth + latWidth, b + longWidth + latWidth, b + longWidth - latWidth, a - longWidth - latWidth, z, color);
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
