using System.Drawing;
using Barebone.Game.Graphics;
using Barebone.Graphics;
using SDL;

namespace Barebone.Game.Sdl
{
    public unsafe class SdlGraphics(SDL_Renderer* rendererPtr) : IPlatformGraphics, IDisposable
    {
        private readonly Dictionary<string, SdlTexture> _textureCache = new();
        
        public void ClearScreen(in Color color)
        {
            SDL3.SDL_SetRenderDrawColor(rendererPtr, color.R, color.G, color.B, color.A);
            SDL3.SDL_RenderClear(rendererPtr);
        }

        public void FillTriangles(in ReadOnlySpan<Vertex> vertices, ITexture? texture)
        {
            var textureInternal = (SdlTexture?)texture;
            var texturePtr = textureInternal == null ? null : textureInternal.Texture;

            fixed (Vertex* ptr = vertices)
            {
                var sdlPtr = (SDL_Vertex*)ptr; // we matched our own Vertex to this. For other platforms, we may have to map instead.
                if (!SDL3.SDL_RenderGeometry(rendererPtr, texturePtr, sdlPtr, vertices.Length, null, 0))
                    throw new SdlException("SDL_RenderGeometry failed: " + SDL3.SDL_GetError());
            }
        }

        public ITexture GetTexture(string assetPath)
        {
            if (!_textureCache.TryGetValue(assetPath, out var t))
                _textureCache.Add(assetPath, t = LoadTexture(assetPath));
            return t;
        }

        private SdlTexture LoadTexture(string assetPath)
        {
            var surface = LoadSurface(assetPath);
            var texture = SDL3.SDL_CreateTextureFromSurface(rendererPtr, surface);
            if (texture == null)
                throw new SdlException("SDL_CreateTextureFromSurface failed: " + SDL3.SDL_GetError());

            SDL3.SDL_ConvertSurface(surface, SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888); // conpatible with struct RGBA because x86 is little-endian
            SDL3.SDL_SetTextureBlendMode(texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL3.SDL_SetTextureColorMod(texture, 255, 255, 255);
            SDL3.SDL_SetTextureAlphaMod(texture, 255);

            return new SdlTexture(surface, texture);
        }

        private static SDL_Surface* LoadSurface(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            var surface = extension switch
            {
                ".png" => SDL3.SDL_LoadPNG(assetPath),
                _ => throw new SdlException("Only PNG textures supported.")
            };
            if (surface == null)
                throw new SdlException("SDL LoadSurfaceX failed: " + SDL3.SDL_GetError());
            return surface;
        }

        public void Dispose()
        {
            foreach (var t in _textureCache.Values)
            {
                SDL3.SDL_DestroyTexture(t.Texture);
                SDL3.SDL_DestroySurface(t.Surface);
            }
        }
    }
}
