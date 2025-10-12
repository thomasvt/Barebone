using System.Numerics;
using System.Runtime.InteropServices;

namespace Barebone.Graphics.Gpu
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct GpuTexVertex(Vector3 Position, GpuColor Color, Vector2 UV);
}
