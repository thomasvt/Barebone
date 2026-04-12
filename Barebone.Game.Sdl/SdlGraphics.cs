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

        public void FillTriangles(in Span<Vertex> vertices)
        {
            //var sdlVertices = stackalloc SDL_Vertex[vertices.Length];

            //for (var i = 0; i < vertices.Length; i++)
            //{
            //    ref readonly var v = ref vertices[i];

            //    sdlVertices[i].position = new() { x = v.Position.X, y = v.Position.Y };
            //    sdlVertices[i].color = new() { a = v.Color.A, b = v.Color.B, g = v.Color.G, r = v.Color.R };
            //}

            fixed (Vertex* ptr = vertices)
            {
                var sdlPtr = (SDL_Vertex*)ptr; // we matched our own Vertex to this. For other platforms, we may have to map instead.
                if (!SDL3.SDL_RenderGeometry(rendererPtr, null, sdlPtr, vertices.Length, null, 0))
                    throw new SdlException("SDL_RenderGeometry failed: " + SDL3.SDL_GetError());
            }
        }
    }
}
