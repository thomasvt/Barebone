using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Graphics
{
    public interface ITexture
    {
        Vector2I Size { get; }
        void ReadPixels(in Span<ColorRgba> pixelBuffer);
        void WritePixels(in Span<ColorRgba> pixelBuffer);
        /// <summary>
        /// Calculates the scale to apply to UVs when projecting this texture onto vertices' world-coordinates to get the given amount of texels in 1 world unit.
        /// </summary>
        Vector2 GetPixelPerfectScale(float texelsPerWorldUnit);
    }
}
