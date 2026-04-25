using Barebone.Geometry;

namespace Barebone.Graphics
{
    public interface ITexture
    {
        Vector2I Size { get; }
        void ReadPixels(in Span<ColorRgba> pixelBuffer);
        void WritePixels(in Span<ColorRgba> pixelBuffer);
    }
}
