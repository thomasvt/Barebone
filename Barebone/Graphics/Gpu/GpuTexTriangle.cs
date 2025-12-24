using System.Runtime.InteropServices;

namespace Barebone.Graphics.Gpu
{
    /// <summary>
    /// Represents a triangle composed of three textured vertices for use in GPU-based rendering operations. The struct's layout is identical to Monogame's VertexPositionColorTexture,
    /// so you can Marshal arrays of this type directly to Monogame buffers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct GpuTexTriangle(GpuTexVertex A, GpuTexVertex B, GpuTexVertex C);
}
