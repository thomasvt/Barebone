using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Pools;
using Barebone.UI.Text;
using Barebone.Graphics.Gpu;
using Barebone.UI.Controls;

namespace Barebone.Graphics
{
    /// <summary>
    /// Renderable trianlge mesh with a submesh per texture. Also has a color-only submesh at index 0.
    /// </summary>
    public class Mesh : Poolable
    {
        private BBList<TexMesh> _subMeshes = null!;

        public ReadOnlySpan<TexMesh> SubMeshes => _subMeshes.AsReadOnlySpan();

        protected internal override void Construct()
        {
            _subMeshes = Pool.Rent<BBList<TexMesh>>();
            _subMeshes.Add(Pool.Rent<TexMesh>()); // first mesh = vertexcolor-only
        }

        protected internal override void Destruct()
        {
            foreach (var mesh in _subMeshes.AsReadOnlySpan())
                mesh.Return();
            _subMeshes.Return();
        }

        public void Clear()
        {
            _subMeshes.InternalArray[0].Clear();
            while (_subMeshes.Count > 1)
            {
                var lastIdx = _subMeshes.Count - 1;
                _subMeshes.InternalArray[lastIdx].Return();
                _subMeshes.SwapRemoveAt(lastIdx);
            }
        }

        public TexMesh GetSubMeshFor(ITexture? texture)
        {
            foreach (var mesh in _subMeshes.AsReadOnlySpan())
            {
                if (mesh.Texture == texture)
                    return mesh;
            }
            var newMesh = Pool.Rent<TexMesh>();
            newMesh.Texture = texture;
            _subMeshes.Add(newMesh);
            return newMesh;
        }

        /// <summary>
        /// Prints text onto the TexMesh using the specified Font.
        /// </summary>
        public Mesh Print(Vector2 position, string text, Color color, Font font, float scale = 1f, float z = 0f)
        {
            var subMesh = GetSubMeshFor(font.Texture);
            font.AppendString(false, subMesh.Triangles, text, color, position, scale, z);
            return this;
        }

        /// <summary>
        /// Prints text onto the TexMesh using the specified Font.
        /// </summary>
        public Mesh Print(Aabb area, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, string text, Color color, Font font, float scale = 1f, float z = 0f)
        {
            var subMesh = GetSubMeshFor(font.Texture);
            var measure = font.MeasureBase(text, scale);

            var x = horizontalAlignment switch
            {
                HorizontalAlignment.Left => area.Left,
                HorizontalAlignment.Right => area.Right - measure.X,
                HorizontalAlignment.Center => area.Center.X - measure.X * 0.5f
            };

            var y = verticalAlignment switch
            {
                VerticalAlignment.Top => area.Top,
                VerticalAlignment.Bottom => area.Bottom + measure.Y,
                VerticalAlignment.Center => area.Center.Y + measure.Y * 0.5f
            };

            font.AppendString(false, subMesh.Triangles, text, color, new(x,y) , scale, z);
            return this;
        }

        public Mesh FillTriangle(in GpuTexVertex a, in GpuTexVertex b, in GpuTexVertex c)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillTriangle(a, b, c);
            return this;
        }

        public Mesh FillTriangle(in Vector3 a, in Vector3 b, in Vector3 c, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillTriangle(a, b, c, color);
            return this;
        }

        public Mesh FillTriangle(in Triangle3 t, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillTriangle(t, color);
            return this;
        }

        public Mesh FillTriangleInZ(in Triangle2 t, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillTriangleInZ(t, z, color);
            return this;
        }

        public Mesh FillQuad(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillQuad(a, b, c, d, color);
            return this;
        }

        public Mesh FillQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillQuadInZ(a, b, c, d, z, color);
            return this;
        }

        public Mesh FillAabbInZ(in Aabb aabb, in float z, in Color color)
        {
            return FillQuadInZ(aabb.MinCorner, aabb.TopLeft, aabb.TopRight, aabb.BottomRight, z, color);
        }

        public Mesh StrokeQuadInZ(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, in float halfWidth, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.StrokeQuadInZ(a, b, c, d, halfWidth, z, color);
            return this;
        }

        public Mesh StrokeAabbInZ(in Aabb aabb, in float halfWidth, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.StrokeAabbInZ(aabb, halfWidth, z, color);
            return this;
        }

        public unsafe Mesh FillPolygonConvexInZ(in Polygon8 polygon, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillPolygonConvexInZ(polygon, z, color);
            return this;
        }

        public Mesh StrokePolygonInZ(in Polygon8 polygon, float strokeWidth, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.StrokePolygonInZ(polygon, strokeWidth, in z, in color);
            return this;
        }

        public Mesh PointInZ(in Vector2 position, in float halfSize, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.PointInZ(position, halfSize, z, color);
            return this;
        }

        public Mesh FillCircleInZ(in Vector2 center, in float radius, in int segmentCount, in float z, in Color colorIn, in Color? colorOut = null, in float angleOffset = 0f)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillCircleInZ(center, radius, segmentCount, z, colorIn, colorOut, angleOffset);
            return this;
        }

        public Mesh FillEllipseInZ(in Vector2 center, in Vector2 radius, in int segmentCount, in float z, in Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.FillEllipseInZ(center, radius, segmentCount, z, color);
            return this;
        }

        public Mesh StrokeRegularPolyInZ(in Vector2 center, in float radius, in float strokeWidth, in int segmentCount, in float z, in Color color, in float angleOffset = 0f)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.StrokeRegularPolyInZ(center, radius, strokeWidth, segmentCount, z, color, angleOffset);
            return this;
        }

        public Mesh LineInZ(Vector2 a, Vector2 b, float halfWidth, float z, Color color)
        {
            var subMesh = GetSubMeshFor(null);
            subMesh.LineInZ(a, b, halfWidth, z, color);
            return this;
        }
    }
}
