using System.Numerics;
using System.Runtime.InteropServices;

namespace Barebone.Graphics.Gpu
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct GpuVertex(Vector3 Position, GpuColor Color);
}
