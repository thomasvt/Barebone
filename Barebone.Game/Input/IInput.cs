using System.Numerics;

namespace Barebone.Game.Input
{
    public interface IInput
    {
        bool JustPressed(KeyboardKey keyboardKey);
        bool JustReleased(KeyboardKey keyboardKey);
        bool IsPressed(KeyboardKey keyboardKey);
        bool JustPressed(MouseButton mouseButton);
        bool JustReleased(MouseButton mouseButton);
        bool IsPressed(MouseButton mouseButton);
        Vector2 MousePosition { get; }

        /// <summary>
        /// Returns the left 2D directional input which is WASD on keyboard or the left thumbstick on gamepads.
        /// </summary>
        Vector2 GetLeftDirectional();

        /// <summary>
        /// Returns the right 2D directional input which is the arrow keys on keyboard or the right thumbstick on gamepads.
        /// </summary>
        Vector2 GetRightDirectional();
    }
}
