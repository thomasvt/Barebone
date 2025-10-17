using Barebone.Geometry;
using Barebone.Graphics.Gpu;

namespace Barebone.Graphics
{
    public interface ITexture : IDisposable
    {
        Vector2I Size { get; }
        void ReadPixels(in GpuColor[] pixelBuffer);
        void WritePixels(in GpuColor[] pixelBuffer);
    }
}
