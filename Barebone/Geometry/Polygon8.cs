using System.Diagnostics.Contracts;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Barebone.Geometry
{
    [InlineArray(8)]
    public struct PointArray8
    {
        private Vector2 P0;
    }

    /// <summary>
    /// DrawPolygon with up to 8 corners as a value-type (inlined in this struct, no array on the heap)
    /// </summary>
    public unsafe struct Polygon8
    {
        public const int MaxVertexCount = 8;
        public int Count;

        private PointArray8 _vertices;

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Count = 3;
            _vertices[0] = p0; _vertices[1] = p1; _vertices[2] = p2;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Count = 4;
            _vertices[0] = p0; _vertices[1] = p1; _vertices[2] = p2; _vertices[3] = p3;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Count = 5;
            _vertices[0] = p0; _vertices[1] = p1; _vertices[2] = p2; _vertices[3] = p3; _vertices[4] = p4;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5)
        {
            Count = 6;
            _vertices[0] = p0; _vertices[1] = p1; _vertices[2] = p2; _vertices[3] = p3; _vertices[4] = p4; _vertices[5] = p5;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6)
        {
            Count = 7;
            _vertices[0] = p0; _vertices[1] = p1; _vertices[2] = p2; _vertices[3] = p3; _vertices[4] = p4; _vertices[5] = p5; _vertices[6] = p6;
        }

        public Polygon8(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6, Vector2 p7)
        {
            Count = 8;
            _vertices[0] = p0; _vertices[1] = p1; _vertices[2] = p2; _vertices[3] = p3; _vertices[4] = p4; _vertices[5] = p5; _vertices[6] = p6; _vertices[7] = p7;
        }

        /// <summary>
        /// Access an individual vertex of the polygon, the index wraps when too large or below 0. For bulk access, use AsSpan() or AsReadOnlySpan().
        /// </summary>
        public Vector2 this[int index]
        {
            get
            {
                index = (index % Count + Count) % Count;

                fixed (Vector2* basePtr = &_vertices[0])
                    return basePtr[index];
            }
            set
            {
                index = (index % Count + Count) % Count;

                fixed (Vector2* basePtr = &_vertices[0])
                    basePtr[index] = value;
            }
        }

        public readonly Span<Vector2> AsSpan()
        {
            fixed (Vector2* basePtr = &_vertices[0])
                return new Span<Vector2>(basePtr, Count);
        }

        public readonly ReadOnlySpan<Vector2> AsReadOnlySpan()
        {
            fixed (Vector2* basePtr = &_vertices[0])
                return new ReadOnlySpan<Vector2>(basePtr, Count);
        }

        /// <summary>
        /// Returns a new tranformed polygon.
        /// </summary>
        [Pure]
        public readonly Polygon8 Transform(in Matrix3x2 transform)
        {
            var result = new Polygon8 { Count = Count };

            for (var i = 0; i < Count; i++)
            {
                result._vertices[i] = Vector2.Transform(_vertices[i], transform);
            }

            return result;
        }

        /// <summary>
        /// Returns a new translated polygon.
        /// </summary>
        [Pure]
        public readonly Polygon8 Translate(in Vector2 translation)
        {
            var result = new Polygon8
            {
                Count = Count
            };

            for (var i = 0; i < Count; i++)
            {
                result._vertices[i] = _vertices[i] + translation;
            }

            return result;
        }

        public readonly void CopyTo(in Span<Vector2> span)
        {
            if (span.Length < Count) throw new ArgumentException($"Array is too small to contain all {Count} corners of this polygon.");
            for (var i = 0; i < Count; i++)
                span[i] = _vertices[i]; 
        }

        public override string ToString()
        {
            var p = _vertices;
            return string.Join(" ", Enumerable.Range(0, Count).Select(i => p[i]));
        }

        [Pure]
        public readonly Polygon8 Rotate(in float radians)
        {
            return Transform(Matrix3x2.CreateRotation(radians));
        }

        [Pure]
        public readonly Polygon8 Scale(in float factor)
        {
            var result = new Polygon8 { Count = Count };

            for (var i = 0; i < Count; i++)
            {
                result._vertices[i] = _vertices[i] * factor;
            }

            return result;
        }

        [Pure]
        public readonly Polygon8 Scale(in Vector2 factor)
        {
            var result = new Polygon8 { Count = Count };

            for (var i = 0; i < Count; i++)
            {
                result._vertices[i] = _vertices[i] * factor;
            }

            return result;
        }

        [Pure]
        public readonly Polygon8 BevelVertex(int index, in float distanceAlongEdges)
        {
            if (Count == MaxVertexCount) throw new InvalidOperationException($"Cannot exceed {MaxVertexCount} corners.");

            index = (index % Count + Count) % Count;
            var indexPrev = (index - 1 + Count) % Count;
            var indexNext = (index + 1) % Count;

            var p = _vertices[index];
            var prev = _vertices[indexPrev];
            var next = _vertices[indexNext];

            var p0 = p + (prev - p).NormalizeOrZero() * distanceAlongEdges;
            var p1 = p + (next - p).NormalizeOrZero() * distanceAlongEdges;

            var result = InsertAt(index+1, p1);
            result._vertices[index] = p0;

            return result;
        }

        public Polygon8 Bevel(float distance)
        {
            var poly = this;
            for (var i = Count-1; i >= 0; i--)
            {
                poly = poly.BevelVertex(i, distance);
            }
            return poly;
        }

        [Pure]
        public readonly Polygon8 InsertAt(int index, Vector2 vertex)
        {
            var p = this;
            p.Count++;

            for (var i = Count; i > index; i--)
                p._vertices[i] = p._vertices[i-1];

            p._vertices[index] = vertex;

            return p;
        }

        public static Polygon8 Square(float size)
        {
            var hs = size * 0.5f;
            return new Polygon8(new(-hs, -hs), new(-hs, hs), new(hs, hs), new(hs, -hs));
        }

        public static Polygon8 Aabb(in Vector2 minCorner, in Vector2 maxCorner)
        {
            var minX = minCorner.X;
            var minY = minCorner.Y;
            var maxX = maxCorner.X;
            var maxY = maxCorner.Y;
            return new Polygon8(new(minX, minY), new(minX, maxY), new(maxX, maxY), new(maxX, minY));
        }

        public float GetCircumpherence()
        {
            if (Count < 2) return 0f;
            var circumpherence = 0f;
            for (var i = 0; i < Count; i++)
            {
                var a = this[i];
                var b = this[(i + 1) % Count];
                circumpherence += Vector2.Distance(a, b);
            }
            return circumpherence;
        }

        public static Polygon8 FromAabb(Aabb aabb)
        {
            return Aabb(aabb.MinCorner, aabb.MaxCorner);
        }

        public Aabb GetAabb()
        {
            var l = _vertices[0].X;
            var r = l;
            var t = _vertices[0].Y;
            var b = t;
            for (var i = 1; i < Count; i++)
            {
                ref var v = ref _vertices[i];
                l = MathF.Min(v.X, l);
                b = MathF.Min(v.Y, b);
                r = MathF.Max(v.X, r);
                t = MathF.Max(v.Y, t);
            }
            return new(new(l, b), new(r, t));
        }
    }
}
