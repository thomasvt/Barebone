using System.Runtime.InteropServices;

namespace Barebone.Graphics.Gpu
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct GpuTriangle(GpuVertex A, GpuVertex B, GpuVertex C);
}
