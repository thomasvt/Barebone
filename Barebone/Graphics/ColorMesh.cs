using System.Numerics;
using System.Runtime.CompilerServices;
using Barebone.Graphics.Gpu;

namespace Barebone.Graphics
{
    public class ColorMesh : MeshBase<ColorMesh, GpuTriangle, GpuVertex>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override GpuTriangle ToTriangle(in GpuVertex a, in GpuVertex b, in GpuVertex c)
        {
            return new GpuTriangle(a, b, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override GpuTriangle ToTriangle(Vector3 a, Vector3 b, Vector3 c, GpuColor gpuColor)
        {
            return new GpuTriangle(new(a, gpuColor), new(b, gpuColor), new(c, gpuColor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override GpuVertex ToVertex(Vector3 a, GpuColor gpuColor)
        {
            return new GpuVertex(a, gpuColor);
        }
    }
}
