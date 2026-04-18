using System.Numerics;
using Barebone.Game.Core;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Geometry;
using SDL;

namespace Barebone.Game.Sdl
{
    public unsafe class SdlPlatform : IPlatform
    {
        private readonly SDL_Window* _windowPtr;
        private readonly SDL_Renderer* _rendererPtr;
        private readonly SdlGraphics _graphics;

        public SdlPlatform(in string windowTitle, Vector2I windowSize)
        {
            if (!SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_EVENTS))
                throw new SdlException($"SDL_Init failed: {SDL3.SDL_GetError()}");

            SDL_Window* windowPtr = null;
            SDL_Renderer* rendererPtr = null;
            var title = (Utf8String)windowTitle;
            if (!SDL3.SDL_CreateWindowAndRenderer(title, windowSize.X, windowSize.Y, SDL_WindowFlags.SDL_WINDOW_RESIZABLE, &windowPtr, &rendererPtr))
                throw new SdlException($"SDL_CreateWindowAndRenderer failed: {SDL3.SDL_GetError()}");

            //if (settings != null)
            //{
            //    if (!SDL3.SDL_SetRenderLogicalPresentation(rendererPtr, settings.Value.Size.X, settings.Value.Size.Y, Map(settings.Value.ScaleMode)))
            //        throw new SdlException($"SDL_SetRenderLogicalPresentation failed: {SDL3.SDL_GetError()}");
            //}

            _windowPtr = windowPtr;
            _rendererPtr = rendererPtr;
            _graphics = new SdlGraphics(_rendererPtr);
        }

        //public ViewportSettings GetViewSettings()
        //{
        //    var lw = 0;
        //    var lh = 0;
        //    var p = SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_DISABLED;
        //    if (!SDL3.SDL_GetRenderLogicalPresentation(_rendererPtr, &lw, &lh, &p))
        //        throw new SdlException("SDL_GetRenderLogicalPresentation failed: " + SDL3.SDL_GetError());

        //    return new(new(lw, lh), Map(p));
        //}

        public Vector2I GetWindowSize()
        {
            var w = 0;
            var h = 0;
            if (!SDL3.SDL_GetWindowSizeInPixels(_windowPtr, &w, &h))
                throw new SdlException("SDL_GetWindowSizeInPixels failed: " + SDL3.SDL_GetError());

            return new(w, h);
        }

        public void ProcessEvents(InputSubSystem input)
        {
            SDL_Event @event;
            while (SDL3.SDL_PollEvent(&@event))
            {
                switch (@event.Type)
                {
                    // keyboard
                    case SDL_EventType.SDL_EVENT_QUIT: IsQuitRequested = true; break;
                    case SDL_EventType.SDL_EVENT_KEY_DOWN: input.KeyboardDown((KeyboardKey)@event.key.key); break;
                    case SDL_EventType.SDL_EVENT_KEY_UP: input.KeyboardUp((KeyboardKey)@event.key.key); break;
                    case SDL_EventType.SDL_EVENT_TEXT_INPUT: break; // todo (auto repeating and keyboard layout support by OS)

                    // mouse
                    case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN: input.MouseDown((MouseButton)@event.button.Button, new Vector2(@event.motion.x, @event.motion.y)); break;
                    case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP: input.MouseUp((MouseButton)@event.button.Button, new Vector2(@event.motion.x, @event.motion.y)); break;
                    case SDL_EventType.SDL_EVENT_MOUSE_MOTION: input.MouseMove(new Vector2(@event.motion.x, @event.motion.y)); break;
                }
            }
        }

        public void Present()
        {
            if (!SDL3.SDL_RenderPresent(_rendererPtr))
                throw new SdlException("SDL_RenderPresent failed: " + SDL3.SDL_GetError());
        }

        public IPlatformGraphics Graphics => _graphics;

        public bool IsQuitRequested { get; private set; }

        private static SDL_RendererLogicalPresentation Map(in LogicalScaleMode scaleMode)
        {
            return scaleMode switch
            {
                LogicalScaleMode.Disabled => SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_DISABLED,
                LogicalScaleMode.ScaleToFit => SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_LETTERBOX,
                LogicalScaleMode.ScaleToFill => SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_OVERSCAN,
                LogicalScaleMode.Stretch => SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_STRETCH,
                LogicalScaleMode.ScaleInteger => SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE,
                _ => throw new ArgumentOutOfRangeException(nameof(scaleMode), scaleMode, null)
            };
        }

        private static LogicalScaleMode Map(in SDL_RendererLogicalPresentation scaleMode)
        {
            return scaleMode switch
            {
                SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_DISABLED => LogicalScaleMode.Disabled,
                SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_LETTERBOX => LogicalScaleMode.ScaleToFit,
                SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_OVERSCAN => LogicalScaleMode.ScaleToFill,
                SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_STRETCH => LogicalScaleMode.Stretch,
                SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE => LogicalScaleMode.ScaleInteger,
                _ => throw new ArgumentOutOfRangeException(nameof(scaleMode), scaleMode, null)
            };
        }

        public void Dispose()
        {
            _graphics.Dispose();
            SDL3.SDL_DestroyRenderer(_rendererPtr);
            SDL3.SDL_DestroyWindow(_windowPtr);
        }
    }
}
