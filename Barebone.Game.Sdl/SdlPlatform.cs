using Barebone.Game.Graphics;
using Barebone.Geometry;
using SDL;

namespace Barebone.Game.Sdl
{
    public unsafe class SdlPlatform : IPlatform
    {
        private readonly SDL_Window* _windowPtr;
        private readonly SDL_Renderer* _rendererPtr;

        public SdlPlatform(in string windowTitle, in Vector2I windowSize)
        {
            if (!SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
                throw new SdlException($"SDL_Init failed: {SDL3.SDL_GetError()}");

            SDL_Window* windowPtr = null;
            SDL_Renderer* rendererPtr = null;
            var title = (Utf8String)windowTitle;
            SDL3.SDL_CreateWindowAndRenderer(title, windowSize.X, windowSize.Y, SDL_WindowFlags.SDL_WINDOW_RESIZABLE, &windowPtr, &rendererPtr);

            _windowPtr = windowPtr;
            _rendererPtr = rendererPtr;
            Graphics = new SdlGraphics(_rendererPtr);
        }

        public void ProcessEvents()
        {
        }

        public void Present()
        {
            SDL3.SDL_RenderPresent(_rendererPtr);
        }

        public IPlatformGraphics Graphics { get; }

        public bool ShouldQuit { get; } = false;

        public void Dispose()
        {
            SDL3.SDL_DestroyRenderer(_rendererPtr);
            SDL3.SDL_DestroyWindow(_windowPtr);
        }
    }
}
