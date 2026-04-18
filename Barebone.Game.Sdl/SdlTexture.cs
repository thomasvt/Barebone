using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using SDL;

namespace Barebone.Game.Sdl
{
    public unsafe class SdlTexture(SDL_Surface* surface, SDL_Texture* texture) : ITexture
    {
        public SDL_Surface* Surface = surface;
        public SDL_Texture* Texture = texture;
        public Vector2I Size => new (Surface->w, Surface->h);

        public void ReadPixels(in Span<ColorRgba> pixelBuffer)
        {
            if (!SDL3.SDL_LockSurface(Surface))
                throw new SdlException("SDL_LockSurface failed: " + SDL3.SDL_GetError());
            var pixels = new ReadOnlySpan<ColorRgba>((ColorRgba*)Surface->pixels, Size.X * Size.Y);
            pixels.CopyTo(pixelBuffer);
            SDL3.SDL_UnlockSurface(Surface);
        }

        public void WritePixels(in Span<ColorRgba> pixelBuffer)
        {
            SDL3.SDL_LockSurface(Surface);
            var pixels = new Span<ColorRgba>((ColorRgba*)Surface->pixels, Size.X * Size.Y);
            pixelBuffer.CopyTo(pixels);
            SDL3.SDL_UnlockSurface(Surface);
        }

        public Vector2 GetScaleForTexelsPerUnit(float texelsPerWorldUnit)
        {
            var w = Surface->w;
            var h = Surface->h;
            return new(texelsPerWorldUnit / w, texelsPerWorldUnit / h);
        }
    }
}
