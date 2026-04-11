using System.Numerics;

namespace Barebone.Game.Input
{
    public class InputSubSystem : IInput
    {
        private readonly ButtonStateTracker<KeyboardKey> _keyboardKeyTracker = new();
        private readonly ButtonStateTracker<MouseButton> _mouseButtonTracker = new();

        /// <summary>
        /// Resets tracking of keys that just changed state.
        /// </summary>
        public void EndFrame()
        {
            _keyboardKeyTracker.EndFrame();
            _mouseButtonTracker.EndFrame();
        }

        public void KeyboardDown(KeyboardKey key)
        {
            _keyboardKeyTracker.Down(key);
        }

        public void KeyboardUp(KeyboardKey key)
        {
            _keyboardKeyTracker.Up(key);
        }

        public void MouseDown(in MouseButton button, in Vector2 position)
        {
            _mouseButtonTracker.Down(button);
            MousePosition = position;
        }

        public void MouseUp(in MouseButton button, in Vector2 position)
        {
            _mouseButtonTracker.Up(button);
            MousePosition = position;
        }

        public void MouseMove(in Vector2 position)
        {
            MousePosition = position;
        }

        public bool JustPressed(KeyboardKey keyboardKey)
        {
            return _keyboardKeyTracker.JustPressed(keyboardKey);
        }

        public bool JustReleased(KeyboardKey keyboardKey)
        {
            return _keyboardKeyTracker.JustReleased(keyboardKey);
        }

        public bool IsPressed(KeyboardKey keyboardKey)
        {
            return _keyboardKeyTracker.IsPressed(keyboardKey);
        }

        public bool JustPressed(MouseButton mouseButton)
        {
            return _mouseButtonTracker.JustPressed(mouseButton);
        }

        public bool JustReleased(MouseButton mouseButton)
        {
            return _mouseButtonTracker.JustReleased(mouseButton);
        }

        public bool IsPressed(MouseButton mouseButton)
        {
            return _mouseButtonTracker.IsPressed(mouseButton);
        }

        public Vector2 MousePosition { get; private set; }
    }
}
