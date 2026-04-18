using System.Drawing;

namespace Barebone.Game.Graphics
{
    public interface IPlatformGraphics
    {
        void ClearScreen(in Color color);
        void FillTriangles(in Span<Vertex> vertices, ITexture? texture);
        ITexture GetTexture(string assetPath);
    }
}
