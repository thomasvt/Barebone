using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Barebone.Game.Graphics;
using Barebone.Geometry;
using Barebone.Graphics;
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

        public void FillPolygon(in Polygon8 polygon, in Color color)
        {
            var pA = polygon[0];
            var sdlA = Unsafe.As<Vector2, SDL_FPoint>(ref pA);
            var vertices = stackalloc SDL_Vertex[(polygon.Count - 2) * 3];
            var cf = ColorF.FromColor(color);
            var sdlColor = Unsafe.As<ColorF, SDL_FColor>(ref cf);
            var i = 0;

            var pB = polygon[1];
            var sdlB = Unsafe.As<Vector2, SDL_FPoint>(ref pB);

            for (var c = 2; c < polygon.Count; c++)
            {
                var pC = polygon[c]; // Polygon indexer supports wrap-around
                var sdlC = Unsafe.As<Vector2, SDL_FPoint>(ref pC);

                vertices[i++] = new SDL_Vertex { color = sdlColor, position = sdlA };
                vertices[i++] = new SDL_Vertex { color = sdlColor, position = sdlB };
                vertices[i++] = new SDL_Vertex { color = sdlColor, position = sdlC };

                sdlB = sdlC;
            }

            if (!SDL3.SDL_RenderGeometry(rendererPtr, null, vertices, i, null, 0))
                throw new SdlException("SDL_RenderGeometry failed: " + SDL3.SDL_GetError());
        }
    }
}
