using Barebone.Geometry;
using Barebone.Input;
using Barebone.Platform.Inputs;
using Barebone.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Vector2 = System.Numerics.Vector2;

namespace Barebone.Monogame
{
    public class XnaInput : IInput
    {
        private MouseState _mousePrevious;
        private MouseState _mouse;
        private KeyboardState _keyboard;
        private KeyboardState _keyboardPrevious;
        private GamePadState _gamePad;
        private GamePadState _gamePadPrevious;
        public InputMode InputMode { get; private set; }
        public Vector2I MousePosition => new(_mouse.X, _mouse.Y);

        public XnaInput(GameWindow window)
        {
            SetInputMode(InputMode.Keyboard);

            window.TextInput += WindowOnTextInput;
            window.KeyDown += WindowOnKeyDown;
            window.KeyUp += WindowOnKeyUp;
        }

        private void WindowOnTextInput(object? sender, TextInputEventArgs e)
        {
            TextInput?.Invoke(e.Character, (KeyboardButton)e.Key);
        }

        private void WindowOnKeyDown(object? sender, InputKeyEventArgs e)
        {
            var control = _keyboard[Keys.LeftControl] == KeyState.Down || _keyboard[Keys.RightControl] == KeyState.Down;
            var shift = _keyboard[Keys.LeftShift] == KeyState.Down || _keyboard[Keys.RightShift] == KeyState.Down;
            var alt = _keyboard[Keys.LeftAlt] == KeyState.Down || _keyboard[Keys.RightAlt] == KeyState.Down;
            var isRepeat = _keyboard[e.Key] == KeyState.Down;
            if (!isRepeat) KeyDown?.Invoke((KeyboardButton)e.Key);
            KeyStroke?.Invoke(new(control, shift, alt, (KeyboardButton)e.Key));
        }

        private void WindowOnKeyUp(object? sender, InputKeyEventArgs e)
        {
            KeyUp?.Invoke((KeyboardButton)e.Key);
        }

        public void Update()
        {
            _keyboardPrevious = _keyboard;
            _keyboard = Keyboard.GetState();

            _mousePrevious = _mouse;
            _mouse = Mouse.GetState();

            _gamePadPrevious = _gamePad;
            _gamePad = GamePad.GetState(0, GamePadDeadZone.Circular);

            if (InputMode != InputMode.GamePad && _gamePad.IsConnected && (_gamePad.Buttons != _gamePadPrevious.Buttons || _gamePad.DPad != _gamePadPrevious.DPad ||
                                                                              _gamePad.ThumbSticks.Left.ToNumerics().Length() > 0.1f || _gamePad.ThumbSticks.Right.ToNumerics().Length() > 0.1f))
                SetInputMode(InputMode.GamePad);
            else if (InputMode != InputMode.Keyboard && !KeysAreEqual(_keyboard.GetPressedKeys(), _keyboardPrevious.GetPressedKeys()))
                SetInputMode(InputMode.Keyboard);
        }

        private static bool KeysAreEqual(Keys[] a, Keys[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        private void SetInputMode(InputMode mode)
        {
            if (mode == InputMode) return;
            InputMode = mode;
        }

        #region Keyboard

        /// <summary>
        /// Did the button just go from not pressed to pressed?
        /// </summary>
        public bool IsJustPressed(KeyboardButton keyboardButton) => IsJust(ButtonState.Pressed, keyboardButton);

        /// <summary>
        /// Did the button just go from pressed to not pressed?
        /// </summary>
        public bool IsJustReleased(KeyboardButton keyboardButton) => IsJust(ButtonState.Released, keyboardButton);

        /// <summary>
        /// Is the button being pressed in the current frame.
        /// </summary>
        public bool IsDown(KeyboardButton keyboardButton) => Is(keyboardButton, ButtonState.Pressed);

        /// <summary>
        /// Is the button not being pressed in the current frame.
        /// </summary>
        public bool IsUp(KeyboardButton keyboardButton) => Is(keyboardButton, ButtonState.Released);

        /// <summary>
        /// Was the button pressed in the previous frame?
        /// </summary>
        public bool WasUp(KeyboardButton keyboardButton) => Was(keyboardButton, ButtonState.Pressed);

        /// <summary>
        /// Was the button released in the previous frame?
        /// </summary>
        public bool WasDown(KeyboardButton keyboardButton) => Was(keyboardButton, ButtonState.Released);


        private bool IsJust(ButtonState state, KeyboardButton keyboardButton)
        {
            var keyState = state == ButtonState.Pressed ? KeyState.Down : KeyState.Up;

            // Barebone uses the same standardised Key values as XNA, so we can map by casting.
            var xnaKey = (Keys)(int)keyboardButton;
            return _keyboardPrevious[xnaKey] != keyState && _keyboard[xnaKey] == keyState;
        }

        private bool Is(KeyboardButton keyboardButton, ButtonState state)
        {
            var keyState = state == ButtonState.Pressed ? KeyState.Down : KeyState.Up;

            // Barebone uses the same standardised Key values as XNA, so we can map by casting.
            var xnaKey = (Keys)(int)keyboardButton;
            return _keyboard[xnaKey] == keyState;
        }

        private bool Was(KeyboardButton keyboardButton, ButtonState state)
        {
            var keyState = state == ButtonState.Pressed ? KeyState.Down : KeyState.Up;

            // Barebone uses the same standardised Key values as XNA, so we can map by casting.
            var xnaKey = (Keys)(int)keyboardButton;
            return _keyboardPrevious[xnaKey] == keyState;
        }

        #endregion

        #region Mouse

        /// <summary>
        /// Did the button just go from not pressed to pressed?
        /// </summary>
        public bool IsJustPressed(MouseButton button) => IsJust(ButtonState.Pressed, button);

        /// <summary>
        /// Did the button just go from pressed to not pressed?
        /// </summary>
        public bool IsJustReleased(MouseButton button) => IsJust(ButtonState.Released, button);

        /// <summary>
        /// Is the button being pressed in the current frame.
        /// </summary>
        public bool IsDown(MouseButton button) => Is(button, ButtonState.Pressed);

        /// <summary>
        /// Is the button not being pressed in the current frame.
        /// </summary>
        public bool IsUp(MouseButton button) => Is(button, ButtonState.Released);

        /// <summary>
        /// Was the button pressed in the previous frame?
        /// </summary>
        public bool WasUp(MouseButton button) => Was(button, ButtonState.Pressed);

        /// <summary>
        /// Was the button released in the previous frame?
        /// </summary>
        public bool WasDown(MouseButton button) => Was(button, ButtonState.Released);


        private bool IsJust(ButtonState state, MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _mousePrevious.LeftButton != state && _mouse.LeftButton == state,
                MouseButton.Right => _mousePrevious.RightButton != state && _mouse.RightButton == state,
                MouseButton.Middle => _mousePrevious.MiddleButton != state && _mouse.MiddleButton == state,

                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

        private bool Is(MouseButton button, ButtonState state)
        {
            return button switch
            {
                MouseButton.Left => _mouse.LeftButton == state,
                MouseButton.Right => _mouse.RightButton == state,
                MouseButton.Middle => _mouse.MiddleButton == state,

                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

        private bool Was(MouseButton button, ButtonState state)
        {
            return button switch
            {
                MouseButton.Left => _mousePrevious.LeftButton == state,
                MouseButton.Right => _mousePrevious.RightButton == state,
                MouseButton.Middle => _mousePrevious.MiddleButton == state,

                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

        #endregion

        #region GamePad

        /// <summary>
        /// Did the button just go from not pressed to pressed?
        /// </summary>
        public bool IsJustPressed(GamePadButton button) => IsJust(ButtonState.Pressed, button);

        /// <summary>
        /// Did the button just go from pressed to not pressed?
        /// </summary>
        public bool IsJustReleased(GamePadButton button) => IsJust(ButtonState.Released, button);

        /// <summary>
        /// Is the button being pressed in the current frame.
        /// </summary>
        public bool IsDown(GamePadButton button) => Is(button, ButtonState.Pressed);

        /// <summary>
        /// Is the button not being pressed in the current frame.
        /// </summary>
        public bool IsUp(GamePadButton button) => Is(button, ButtonState.Released);

        /// <summary>
        /// Was the button pressed in the previous frame?
        /// </summary>
        public bool WasUp(GamePadButton button) => Was(button, ButtonState.Pressed);

        /// <summary>
        /// Was the button released in the previous frame?
        /// </summary>
        public bool WasDown(GamePadButton button) => Was(button, ButtonState.Released);


        private bool IsJust(ButtonState state, GamePadButton button)
        {
            return button switch
            {
                GamePadButton.A => _gamePadPrevious.Buttons.A != state && _gamePad.Buttons.A == state,
                GamePadButton.B => _gamePadPrevious.Buttons.B != state && _gamePad.Buttons.B == state,
                GamePadButton.X => _gamePadPrevious.Buttons.X != state && _gamePad.Buttons.X == state,
                GamePadButton.Y => _gamePadPrevious.Buttons.Y != state && _gamePad.Buttons.Y == state,
                GamePadButton.ShoulderL => _gamePadPrevious.Buttons.LeftShoulder != state && _gamePad.Buttons.LeftShoulder == state,
                GamePadButton.ShoulderR => _gamePadPrevious.Buttons.RightShoulder != state && _gamePad.Buttons.RightShoulder == state,
                GamePadButton.TriggerL => state == ButtonState.Pressed ? _gamePadPrevious.Triggers.Left == 0 && _gamePad.Triggers.Left > 0 : _gamePadPrevious.Triggers.Left > 0 && _gamePad.Triggers.Left == 0,
                GamePadButton.TriggerR => state == ButtonState.Pressed ? _gamePadPrevious.Triggers.Right == 0 && _gamePad.Triggers.Right > 0 : _gamePadPrevious.Triggers.Right > 0 && _gamePad.Triggers.Right == 0,
                GamePadButton.DPadLeft => _gamePadPrevious.DPad.Left != state && _gamePad.DPad.Left == state,
                GamePadButton.DPadRight => _gamePadPrevious.DPad.Right != state && _gamePad.DPad.Right == state,
                GamePadButton.DPadUp => _gamePadPrevious.DPad.Up != state && _gamePad.DPad.Up == state,
                GamePadButton.DPadDown => _gamePadPrevious.DPad.Down != state && _gamePad.DPad.Down == state,

                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

        private bool Is(GamePadButton button, ButtonState state)
        {
            return button switch
            {
                GamePadButton.A => _gamePad.Buttons.A == state,
                GamePadButton.B => _gamePad.Buttons.B == state,
                GamePadButton.X => _gamePad.Buttons.X == state,
                GamePadButton.Y => _gamePad.Buttons.Y == state,
                GamePadButton.ShoulderL => _gamePad.Buttons.LeftShoulder == state,
                GamePadButton.ShoulderR => _gamePad.Buttons.RightShoulder == state,
                GamePadButton.TriggerL => state == ButtonState.Pressed ? _gamePad.Triggers.Left > 0 : _gamePad.Triggers.Left == 0,
                GamePadButton.TriggerR => state == ButtonState.Pressed ? _gamePad.Triggers.Right > 0 : _gamePad.Triggers.Right == 0,

                GamePadButton.DPadLeft => _gamePad.DPad.Left == state,
                GamePadButton.DPadRight => _gamePad.DPad.Right == state,
                GamePadButton.DPadUp => _gamePad.DPad.Up == state,
                GamePadButton.DPadDown => _gamePad.DPad.Down == state,

                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

        private bool Was(GamePadButton button, ButtonState state)
        {
            return button switch
            {
                GamePadButton.A => _gamePadPrevious.Buttons.A == state,
                GamePadButton.B => _gamePadPrevious.Buttons.B == state,
                GamePadButton.X => _gamePadPrevious.Buttons.X == state,
                GamePadButton.Y => _gamePadPrevious.Buttons.Y == state,
                GamePadButton.ShoulderL => _gamePadPrevious.Buttons.LeftShoulder == state,
                GamePadButton.ShoulderR => _gamePadPrevious.Buttons.RightShoulder == state,
                GamePadButton.TriggerL => state == ButtonState.Pressed ? _gamePadPrevious.Triggers.Left > 0 : _gamePadPrevious.Triggers.Left == 0,
                GamePadButton.TriggerR => state == ButtonState.Pressed ? _gamePadPrevious.Triggers.Right > 0 : _gamePadPrevious.Triggers.Right == 0,
                GamePadButton.DPadLeft => _gamePadPrevious.DPad.Left == state,
                GamePadButton.DPadRight => _gamePadPrevious.DPad.Right == state,
                GamePadButton.DPadUp => _gamePadPrevious.DPad.Up == state,
                GamePadButton.DPadDown => _gamePadPrevious.DPad.Down == state,

                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

        #endregion

        public bool DidMouseScrollUp()
        {
            return _mousePrevious.ScrollWheelValue < _mouse.ScrollWheelValue;
        }

        public bool DidMouseScrollDown()
        {
            return _mousePrevious.ScrollWheelValue > _mouse.ScrollWheelValue;
        }

        /// <summary>
        /// Gets the left gamepad thumbstick.
        /// </summary>
        public Vector2 GetLeftStick()
        {
            return _gamePad.ThumbSticks.Left.ToNumerics();
        }

        /// <summary>
        /// Gets the right gamepad thumbstick.
        /// </summary>
        public Vector2 GetRightStick()
        {
            return _gamePad.ThumbSticks.Right.ToNumerics();
        }

        /// <summary>
        /// Gets the left gamepad trigger's value.
        /// </summary>
        public float GetLeftTrigger()
        {
            return _gamePad.Triggers.Left;
        }

        /// <summary>
        /// Gets the right gamepad trigger's value.
        /// </summary>
        public float GetRightTrigger()
        {
            return _gamePad.Triggers.Right;
        }

        /// <summary>
        /// Gets the WASD input as a normalized vector.
        /// </summary>
        public Vector2 GetKeyboardWasd()
        {
            var keyboardInput = new Vector2(GetKeyAxis(KeyboardButton.A, KeyboardButton.D), GetKeyAxis(KeyboardButton.S, KeyboardButton.W));
            if (keyboardInput.X != 0 && keyboardInput.Y != 0)
                return Vector2.Normalize(keyboardInput);
            return keyboardInput;
        }

        /// <summary>
        /// Gets the keyboard arrows input as a normalized vector.
        /// </summary>
        public Vector2 GetKeyboardArrows()
        {
            var keyboardInput = new Vector2(GetKeyAxis(KeyboardButton.Left, KeyboardButton.Right), GetKeyAxis(KeyboardButton.Down, KeyboardButton.Up));
            if (keyboardInput.X != 0 && keyboardInput.Y != 0)
                return Vector2.Normalize(keyboardInput);
            return keyboardInput;
        }

        public float GetKeyAxis(KeyboardButton decrease, KeyboardButton increase)
        {
            if (Is(decrease, ButtonState.Pressed)) return -1;
            if (Is(increase, ButtonState.Pressed)) return 1;
            return 0;
        }

        public Vector2 GetKeyAxis2(KeyboardButton decreaseX, KeyboardButton increaseX, KeyboardButton decreaseY, KeyboardButton increaseY)
        {
            return new Vector2(GetKeyAxis(decreaseX, increaseX), GetKeyAxis(decreaseY, increaseY));
        }

        public event Action<char, KeyboardButton>? TextInput;
        public event Action<KeyStrokeEvent>? KeyStroke;
        public event Action<KeyboardButton>? KeyDown;
        public event Action<KeyboardButton>? KeyUp;
    }
}