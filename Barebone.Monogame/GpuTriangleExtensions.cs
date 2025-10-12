using System.Runtime.InteropServices;
using Barebone.Graphics.Gpu;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    public static class GpuTriangleExtensions
    {
        public static void MapToXna(this ReadOnlySpan<GpuTriangle> triangles, Span<VertexPositionColor> destination)
        {
            var destSpan = MemoryMarshal.Cast<VertexPositionColor, byte>(destination);
            var srcSpan = MemoryMarshal.Cast<GpuTriangle, byte>(triangles);
            srcSpan.CopyTo(destSpan);
        }

        public static void MapToXna(this ReadOnlySpan<GpuTexTriangle> triangles, Span<VertexPositionColorTexture> destination)
        {
            var destSpan = MemoryMarshal.Cast<VertexPositionColorTexture, byte>(destination);
            var srcSpan = MemoryMarshal.Cast<GpuTexTriangle, byte>(triangles);
            srcSpan.CopyTo(destSpan);
        }
    }
}
