using System.Diagnostics;
using System.Numerics;
using Barebone.Game.Core;
using Barebone.Game.Graphics;
using Barebone.Game.Input;
using Barebone.Geometry;
using SDL;
using YamlDotNet.Core.Tokens;

namespace Barebone.Game.Sdl
{
    public unsafe class SdlPlatform : IPlatform
    {
        private readonly SDL_Window* _windowPtr;
        private readonly SDL_Renderer* _rendererPtr;

        public SdlPlatform(in string windowTitle, in Vector2I windowSize, in Vector2I logicalSize, in LogicalScaleMode scaleMode)
        {
            if (!SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_EVENTS))
                throw new SdlException($"SDL_Init failed: {SDL3.SDL_GetError()}");

            SDL_Window* windowPtr = null;
            SDL_Renderer* rendererPtr = null;
            var title = (Utf8String)windowTitle;
            SDL3.SDL_CreateWindowAndRenderer(title, windowSize.X, windowSize.Y, SDL_WindowFlags.SDL_WINDOW_RESIZABLE, &windowPtr, &rendererPtr);

            SDL3.SDL_SetRenderLogicalPresentation(rendererPtr, logicalSize.X, logicalSize.Y, ToSDL3(scaleMode));

            _windowPtr = windowPtr;
            _rendererPtr = rendererPtr;
            Graphics = new SdlGraphics(_rendererPtr);
        }

        private SDL_RendererLogicalPresentation ToSDL3(in LogicalScaleMode scaleMode)
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
            SDL3.SDL_RenderPresent(_rendererPtr);
        }

        public IPlatformGraphics Graphics { get; }

        public bool IsQuitRequested { get; private set; }

        public void Dispose()
        {
            SDL3.SDL_DestroyRenderer(_rendererPtr);
            SDL3.SDL_DestroyWindow(_windowPtr);
        }

        public Action<KeyboardKey> KeyDown;
        public Action<KeyboardKey> KeyUp;
    }
}
