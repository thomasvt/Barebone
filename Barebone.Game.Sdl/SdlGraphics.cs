using System.Drawing;
using Barebone.Game.Graphics;
using SDL;

namespace Barebone.Game.Sdl
{
    public unsafe class SdlGraphics(SDL_Renderer* rendererPtr) : IPlatformGraphics
    {
        public void ClearScreen(in Color color)
        {
            SDL3.SDL_SetRenderDrawColor(rendererPtr, color.R, color.G, color.B, color.A);
            SDL3.SDL_RenderClear(rendererPtr);
        }
    }
}
