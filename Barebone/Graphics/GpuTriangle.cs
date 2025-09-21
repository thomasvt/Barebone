using System.Numerics;
using System.Runtime.InteropServices;

namespace Barebone.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public record struct GpuColor(uint Packed);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct GpuVertex(Vector3 Position, GpuColor Color);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct GpuTriangle(GpuVertex A, GpuVertex B, GpuVertex C);

}
