using System.Runtime.InteropServices;

namespace Barebone.Graphics.Gpu
{
    [StructLayout(LayoutKind.Sequential)]
    public record struct GpuColor(uint Packed);
}
