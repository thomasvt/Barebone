using System.Numerics;

namespace Barebone.UI.Text
{
    public struct GlyphQuad
    {
        public readonly Vector2 QuadMin;
        public readonly Vector2 QuadMax;
        public readonly Vector2 UVMin;
        public readonly Vector2 UVMax;

        public GlyphQuad(Vector2 quadMin, Vector2 quadMax, Vector2 uvMin, Vector2 uvMax)
        {
            QuadMin = quadMin;
            QuadMax = quadMax;
            UVMin = uvMin;
            UVMax = uvMax;
        }
    }
}
