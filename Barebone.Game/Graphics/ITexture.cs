using System.Numerics;

namespace Barebone.Game.Graphics
{
    public interface ITexture
    {
        /// <summary>
        /// Calculates the scale to apply to UVs when projecting this texture onto vertices' world-coordinates to get the given amount of texels in 1 world unit.
        /// </summary>
        Vector2 GetPixelPerfectScale(float texelsPerWorldUnit);
    }
}
