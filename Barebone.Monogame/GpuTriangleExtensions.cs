using System.Runtime.InteropServices;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    public static class GpuTriangleExtensions
    {
        public static void CopyTo(this ReadOnlySpan<GpuTriangle> triangles, Span<VertexPositionColor> destination)
        {
            var destSpan = MemoryMarshal.Cast<VertexPositionColor, byte>(destination);
            var srcSpan = MemoryMarshal.Cast<GpuTriangle, byte>(triangles);
            srcSpan.CopyTo(destSpan);
        }
    }
}
