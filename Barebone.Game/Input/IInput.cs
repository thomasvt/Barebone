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
    }
}
