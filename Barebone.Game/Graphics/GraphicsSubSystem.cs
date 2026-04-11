using System.Drawing;

namespace Barebone.Game.Graphics
{
    internal class GraphicsSubSystem(IPlatformGraphics pg) : IGraphics
    {
        public void BeginFrame()
        {
        }

        public void ClearScreen(in Color color)
        {
            pg.ClearScreen(color);
        }

        public void EndFrame()
        {
        }
    }
}
