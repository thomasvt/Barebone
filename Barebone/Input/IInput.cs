using System.Numerics;
using Barebone.Geometry;
using Barebone.Platform.Inputs;

namespace Barebone.Input;

public record struct KeyStrokeEvent(bool Control, bool Shift, bool Alt, Button Button);
public interface IInput
{
    Vector2I MousePosition { get; }
    InputMode InputMode { get; }
    void Update();
    bool IsJustPressed(Button button);
    bool IsJustReleased(Button button);
    bool IsDown(Button button);
    bool IsUp(Button button);
    bool WasUp(Button button);
    bool WasDown(Button button);
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

    float GetKeyAxis(Button decrease, Button increase);

    /// <summary>
    /// OS managed auto-repeated text input according to keyboard locale and text-input mechanics.
    /// </summary>
    event Action<char, Button>? TextInput;

    /// <summary>
    /// OS managed auto-repeated input of individual keyboard buttons. Mind that these partially overlap with TextInput events: this event gives you the lower level button strikes, TextInput gives you higher level intepretation of key combinations into text-input.
    /// </summary>
    event Action<KeyStrokeEvent>? KeyStroke;

    event Action<Button>? KeyDown;
    event Action<Button>? KeyUp;
}