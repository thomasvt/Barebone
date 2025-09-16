using System.Numerics;

namespace Barebone.Geometry
{
    public partial struct Polygon8
    {
        public static PolygonIntersection Intersect(Polygon8 a, Polygon8 b)
        {
            var result = new PolygonIntersection { IsOverlap = true, MTV = Vector2.Zero };

            var minOverlap = float.MaxValue;
            var smallestAxis = Vector2.Zero;

            // Loop over both polygons' edges
            for (var polyIndex = 0; polyIndex < 2; polyIndex++)
            {
                var poly = polyIndex == 0 ? a : b;

                for (var i = 0; i < poly.Count; i++)
                {
                    var p1 = poly._vertices[i];
                    var p2 = poly._vertices[(i + 1) % poly.Count];

                    // Edge and perpendicular axis
                    var edge = p2 - p1;
                    var axis = new Vector2(-edge.Y, edge.X);
                    axis = Vector2.Normalize(axis);

                    ProjectPolygon(a, axis, out float minA, out float maxA);
                    ProjectPolygon(b, axis, out float minB, out float maxB);

                    // Check overlap
                    var overlap = IntervalOverlap(minA, maxA, minB, maxB);
                    if (overlap <= 0)
                    {
                        // Separating axis found → no intersection
                        result.IsOverlap = false;
                        result.MTV = Vector2.Zero;
                        return result;
                    }

                    // Keep the smallest overlap (for Distance)
                    if (overlap < minOverlap)
                    {
                        minOverlap = overlap;
                        smallestAxis = axis;

                        // Ensure Distance points from A → B
                        Vector2 centerA = Vector2.Zero, centerB = Vector2.Zero;
                        foreach (var v in a.AsReadOnlySpan()) centerA += v;
                        foreach (var v in b.AsReadOnlySpan()) centerB += v;
                        centerA /= a.Count;
                        centerB /= b.Count;

                        Vector2 direction = centerB - centerA;
                        if (Vector2.Dot(direction, smallestAxis) < 0)
                            smallestAxis = -smallestAxis;
                    }
                }
            }

            result.MTV = smallestAxis * minOverlap;
            return result;
        }

        /// <summary>
        /// Gets an approximation of the intersection center, based on the result of <see cref="Intersect"/>.
        /// </summary>
        public static unsafe Vector2 GetIntersectionPoint(in Polygon8 a, in Polygon8 b, in PolygonIntersection polygonIntersection)
        {
            var contacts = stackalloc Vector2[a.Count + b.Count];
            var contactCount = 0;

            // Normalize the axis for consistent projection
            var axis = Vector2.Normalize(polygonIntersection.MTV);

            // Project both polygons
            ProjectPolygon(a, axis, out var minA, out var maxA);
            ProjectPolygon(b, axis, out var minB, out var maxB);

            var overlapMin = Math.Max(minA, minB);
            var overlapMax = Math.Min(maxA, maxB);

            CollectContacts(a);
            CollectContacts(b);

            // Fallback: if no vertices directly in overlap, just return midpoint
            if (contactCount == 0)
                return (a._vertices[0] + b._vertices[0]) * 0.5f;

            // Average the contact vertices
            var sum = Vector2.Zero;
            for (var i = 0; i <  contactCount; i++)
                sum += contacts[i];
            return sum / contactCount;

            void CollectContacts(in Polygon8 poly)
            {
                foreach (var v in poly.AsReadOnlySpan())
                {
                    var proj = Vector2.Dot(axis, v);
                    if (proj >= overlapMin - 1e-6f && proj <= overlapMax + 1e-6f)
                    {
                        contacts[contactCount++] = v;
                    }
                }
            }
        }

        /// <summary>
        /// Project polygon onto axis and return [min, max] projection
        /// </summary>
        private static void ProjectPolygon(Polygon8 poly, Vector2 axis, out float min, out float max)
        {
            var dot = Vector2.Dot(axis, poly._vertices[0]);
            min = dot;
            max = dot;

            for (var i = 1; i < poly.Count; i++)
            {
                dot = Vector2.Dot(axis, poly._vertices[i]);
                if (dot < min) min = dot;
                if (dot > max) max = dot;
            }
        }

        /// <summary>
        /// Returns overlap between two intervals, or <=0 if none
        /// </summary>
        private static float IntervalOverlap(float minA, float maxA, float minB, float maxB)
        {
            return Math.Min(maxA, maxB) - Math.Max(minA, minB);
        }
    }
}
