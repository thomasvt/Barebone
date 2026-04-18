using System.Drawing;
using Barebone.Graphics;

namespace Barebone.Game.Graphics
{
    public interface IPlatformGraphics
    {
        void ClearScreen(in Color color);
        void FillTriangles(in ReadOnlySpan<Vertex> vertices, ITexture? texture);
        ITexture GetTexture(string assetPath);
    }
}
