using Barebone.Geometry;
using Box2dNet.Interop;

namespace Barebone.Box2d
{
    public static class Polygon8Extensions
    {
        public static b2ShapeProxy ToB2ShapeProxy(this Polygon8 poly)
        {
            var proxy = new b2ShapeProxy();
            proxy.count = poly.Count;

            var result = new b2ShapeProxy
            {
                count = poly.Count,
                points0 = poly[0],
                points1 = poly[1],
                points2 = poly[2]
            };

            if (poly.Count <= 3) return result;
            result.points3 = poly[3];
            if (poly.Count == 4) return result;
            result.points4 = poly[4];
            if (poly.Count == 5) return result;
            result.points5 = poly[5];
            if (poly.Count == 6) return result;
            result.points6 = poly[6];
            if (poly.Count == 7) return result;
            result.points7 = poly[7];
            return result;
        }
    }
}
