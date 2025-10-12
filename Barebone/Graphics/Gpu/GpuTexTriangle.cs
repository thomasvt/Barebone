using System.Runtime.InteropServices;

namespace Barebone.Graphics.Gpu
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct GpuTexTriangle(GpuTexVertex A, GpuTexVertex B, GpuTexVertex C);
}
