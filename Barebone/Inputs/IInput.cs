using System.Numerics;

namespace Barebone.Inputs;

public interface IInput
{
    Vector2 MousePosition { get; }
    InputMode InputMode { get; }
    void Update();
    bool IsJustPressed(Button button);
    bool IsJustReleased(Button button);
    bool IsPressed(Button button);
    bool IsReleased(Button button);
    bool DidMouseScrollUp();
    bool DidMouseScrollDown();

    /// <summary>
    /// Gets the left directional input: the left gamepad thumbstick or keyboard WASD buttons. In case of keyboard, the result is a normalized vector.
    /// </summary>
    Vector2 GetLeftStick();

    /// <summary>
    /// Gets the right directional input: the right gamepad thumbstick or keyboard arrow buttons. In case of keyboard, the result is a normalized vector.
    /// </summary>
    Vector2 GetRightStick();

    /// <summary>
    /// Gets the left gamepad trigger's value.
    /// </summary>
    float GetLeftTrigger();

    /// <summary>
    /// Gets the right gamepad trigger's value.
    /// </summary>
    float GetRightTrigger();

    /// <summary>
    /// Gets the WASD input as a normalized vector.
    /// </summary>
    Vector2 GetKeyboardWasd();

    /// <summary>
    /// Gets the keyboard arrows input as a normalized vector.
    /// </summary>
    Vector2 GetKeyboardArrows();
}