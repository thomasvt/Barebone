using System.Numerics;
using Barebone.Inputs;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

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
        public Vector2 MousePosition => new(_mouse.X, _mouse.Y);

        public XnaInput()
        {
            SetInputMode(InputMode.Keyboard);
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

        public bool IsJustPressed(Button button) => IsJust(ButtonState.Pressed, button);
        public bool IsJustReleased(Button button) => IsJust(ButtonState.Released, button);
        public bool IsPressed(Button button) => Is(button, ButtonState.Pressed);
        public bool IsReleased(Button button) => Is(button, ButtonState.Released);

        private bool IsJust(ButtonState state, Button button)
        {
            var keyState = state == ButtonState.Pressed ? KeyState.Down : KeyState.Up;

            return button switch
            {
                Button.MouseLeft => _mousePrevious.LeftButton != state && _mouse.LeftButton == state,
                Button.MouseRight => _mousePrevious.RightButton != state && _mouse.RightButton == state,
                Button.MouseMiddle => _mousePrevious.MiddleButton != state && _mouse.MiddleButton == state,

                Button.W => _keyboardPrevious[Keys.W] != keyState && _keyboard[Keys.W] == keyState,
                Button.A => _keyboardPrevious[Keys.A] != keyState && _keyboard[Keys.A] == keyState,
                Button.S => _keyboardPrevious[Keys.S] != keyState && _keyboard[Keys.S] == keyState,
                Button.D => _keyboardPrevious[Keys.D] != keyState && _keyboard[Keys.D] == keyState,

                Button.PadA => _gamePadPrevious.Buttons.A != state && _gamePad.Buttons.A == state,
                Button.PadB => _gamePadPrevious.Buttons.B != state && _gamePad.Buttons.B == state,
                Button.PadX => _gamePadPrevious.Buttons.X != state && _gamePad.Buttons.X == state,
                Button.PadY => _gamePadPrevious.Buttons.Y != state && _gamePad.Buttons.Y == state,
                Button.PadShoulderL => _gamePadPrevious.Buttons.LeftShoulder != state && _gamePad.Buttons.LeftShoulder == state,
                Button.PadShoulderR => _gamePadPrevious.Buttons.RightShoulder != state && _gamePad.Buttons.RightShoulder == state,
                Button.PadTriggerL => state == ButtonState.Pressed ? _gamePadPrevious.Triggers.Left == 0 && _gamePad.Triggers.Left > 0 : _gamePadPrevious.Triggers.Left > 0 && _gamePad.Triggers.Left == 0,
                Button.PadTriggerR => state == ButtonState.Pressed ? _gamePadPrevious.Triggers.Right == 0 && _gamePad.Triggers.Right > 0 : _gamePadPrevious.Triggers.Right > 0 && _gamePad.Triggers.Right == 0,

                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

        private bool Is(Button button, ButtonState state)
        {
            var keyState = state == ButtonState.Pressed ? KeyState.Down : KeyState.Up;

            return button switch
            {
                Button.MouseLeft => _mouse.LeftButton == state,
                Button.MouseRight => _mouse.RightButton == state,
                Button.MouseMiddle => _mouse.MiddleButton == state,

                Button.W => _keyboard[Keys.W] == keyState,
                Button.A => _keyboard[Keys.A] == keyState,
                Button.S => _keyboard[Keys.S] == keyState,
                Button.D => _keyboard[Keys.D] == keyState,
                Button.Space => _keyboard[Keys.Space] == keyState,

                Button.PadA => _gamePad.Buttons.A == state,
                Button.PadB => _gamePad.Buttons.B == state,
                Button.PadX => _gamePad.Buttons.X == state,
                Button.PadY => _gamePad.Buttons.Y == state,
                Button.PadShoulderL => _gamePad.Buttons.LeftShoulder == state,
                Button.PadShoulderR => _gamePad.Buttons.RightShoulder == state,
                Button.PadTriggerL => state == ButtonState.Pressed ? _gamePad.Triggers.Left > 0 : _gamePad.Triggers.Left == 0,
                Button.PadTriggerR => state == ButtonState.Pressed ? _gamePad.Triggers.Right > 0 : _gamePad.Triggers.Right == 0,


                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
        }

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
            var keyboardInput = new Vector2(GetKeyAxis(Keys.A, Keys.D), GetKeyAxis(Keys.S, Keys.W));
            if (keyboardInput.X != 0 && keyboardInput.Y != 0)
                return Vector2.Normalize(keyboardInput);
            return keyboardInput;
        }

        /// <summary>
        /// Gets the keyboard arrows input as a normalized vector.
        /// </summary>
        public Vector2 GetKeyboardArrows()
        {
            var keyboardInput = new Vector2(GetKeyAxis(Keys.Left, Keys.Right), GetKeyAxis(Keys.Down, Keys.Up));
            if (keyboardInput.X != 0 && keyboardInput.Y != 0)
                return Vector2.Normalize(keyboardInput);
            return keyboardInput;
        }

        private float GetKeyAxis(Keys decrease, Keys increase)
        {
            if (_keyboard.IsKeyDown(decrease)) return -1;
            if (_keyboard.IsKeyDown(increase)) return 1;
            return 0;
        }
    }
}