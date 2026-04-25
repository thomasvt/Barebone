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
        public Vector2 GetLeftDirectional()
        {
            return new(GetAxis(KeyboardKey.A, KeyboardKey.D),
                GetAxis(KeyboardKey.W, KeyboardKey.S));
        }

        public Vector2 GetRightDirectional()
        {
            return new(
                GetAxis(KeyboardKey.Left, KeyboardKey.Right),
                GetAxis(KeyboardKey.Down, KeyboardKey.Up));
        }

        private float GetAxis(KeyboardKey decrease, KeyboardKey increase)
        {
            return IsPressed(decrease) 
                ? -1f 
                : IsPressed(increase)
                    ? 1
                    : 0;
        }
    }
}
