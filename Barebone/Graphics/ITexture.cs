using Barebone.Geometry;

namespace Barebone.Graphics
{
    public interface ITexture
    {
        Vector2I Size { get; }
        void ReadPixels(in ColorRgba[] pixelBuffer);
        void WritePixels(in ColorRgba[] pixelBuffer);
    }
}
