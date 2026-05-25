using Barebone.Geometry;

namespace Barebone.Graphics
{
    public interface ITexture
    {
        Vector2I Size { get; }
        void ReadPixels(in Color8[] pixelBuffer);
        void WritePixels(in Color8[] pixelBuffer);
    }
}
