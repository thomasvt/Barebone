using System.Numerics;
using Barebone.Geometry;
using Barebone.Platform.Inputs;

namespace Barebone.Input;

public record struct KeyStrokeEvent(bool Control, bool Shift, bool Alt, KeyboardButton KeyboardButton);
public interface IInput
{
    Vector2I MousePosition { get; }
    InputMode InputMode { get; }
    void Update();
    bool IsJustPressed(KeyboardButton keyboardButton);
    bool IsJustReleased(KeyboardButton keyboardButton);
    bool IsDown(KeyboardButton keyboardButton);
    bool IsUp(KeyboardButton keyboardButton);
    bool WasUp(KeyboardButton keyboardButton);
    bool WasDown(KeyboardButton keyboardButton);

    bool IsJustPressed(MouseButton button);
    bool IsJustReleased(MouseButton button);
    bool IsDown(MouseButton button);
    bool IsUp(MouseButton button);
    bool WasUp(MouseButton button);
    bool WasDown(MouseButton button);
    bool DidMouseScrollUp();
    bool DidMouseScrollDown();

    bool IsJustPressed(GamePadButton button);
    bool IsJustReleased(GamePadButton button);
    bool IsDown(GamePadButton button);
    bool IsUp(GamePadButton button);
    bool WasUp(GamePadButton button);
    bool WasDown(GamePadButton button);

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

    float GetKeyAxis(KeyboardButton decrease, KeyboardButton increase);

    Vector2 GetKeyAxis2(KeyboardButton decreaseX, KeyboardButton increaseX, KeyboardButton decreaseY, KeyboardButton increaseY);

    /// <summary>
    /// OS managed auto-repeated text input according to keyboard locale and text-input mechanics.
    /// </summary>
    event Action<char, KeyboardButton>? TextInput;

    /// <summary>
    /// OS managed auto-repeated input of individual keyboard buttons. Mind that these partially overlap with TextInput events: this event gives you the lower level button strikes, TextInput gives you higher level intepretation of key combinations into text-input.
    /// </summary>
    event Action<KeyStrokeEvent>? KeyStroke;

    event Action<KeyboardButton>? KeyDown;
    event Action<KeyboardButton>? KeyUp;
    
}