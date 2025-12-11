using System.Drawing;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Cameras;
using Barebone.Input;
using Barebone.Messaging;
using Barebone.UI.Controls;
using Barebone.UI.Text;

namespace Barebone.UI
{
    /// <summary>
    /// Root object for a UI.
    /// </summary>
    public class UserInterface : UIControl, IDisposable
    {
        private readonly IInput _input;
        private readonly OrthographicCamera _camera = new();
        private UIControl? _focussedControl;
        private UIControl? _mouseCapturer;
        private Vector2I _previousMousePosition;
        private readonly List<UIControl> _mouseHoverChain = new();

        public UserInterface(Font defaultFont, IInput input, Clock clock, IClipboard clipboard) : base(null!)
        {
            _input = input;
            DefaultFont = defaultFont;
            input.TextInput += ProcessTypeInput;
            input.KeyStroke += ProcessKeyStroke;
            input.KeyDown += ProcessKeyDown;
            input.KeyUp += ProcessKeyUp;
            Clock = clock;
            Clipboard = clipboard;
            MessageBus = new MessageBus();
        }

        public void AddChild(UIControl editorUI)
        {
            Children.Add(editorUI);
        }

        protected override void Arrange()
        {
            _camera.For2DCanvas(Viewport.Height, 10);
            base.Arrange();
        }

        public void Update()
        {
            ProcessMouseEvents();
            UpdateInternal();
        }

        private void ProcessMouseEvents()
        {
            var mousePosition = _input.MousePosition;

            if (_previousMousePosition != mousePosition)
            {
                ScreenPick(mousePosition, _mouseHoverChain);
                DispatchMouseMoveEvent(_previousMousePosition, mousePosition);
                _previousMousePosition = mousePosition;
            }

            if (_input.IsJustPressed(Platform.Inputs.Button.MouseLeft))
                DispatchMouseButtonChange(mousePosition, MouseButton.Left, ButtonState.Pressed);
            else if (_input.IsJustReleased(Platform.Inputs.Button.MouseLeft))
                DispatchMouseButtonChange(mousePosition, MouseButton.Left, ButtonState.Released);
            else if (_input.IsJustPressed(Platform.Inputs.Button.MouseRight))
                DispatchMouseButtonChange(mousePosition, MouseButton.Right, ButtonState.Pressed);
            else if (_input.IsJustReleased(Platform.Inputs.Button.MouseRight))
                DispatchMouseButtonChange(mousePosition, MouseButton.Right, ButtonState.Released);
        }

        public void ScreenPick(in Vector2I mousePosition, in List<UIControl> controlChain)
        {
            // we first retest the existing hover chain and clear IsMouseOver for controls that are no longer hovered.
            var isMouseOver = true;
            foreach (var control in controlChain)
            {
                if (!isMouseOver || !control.Viewport.Contains(mousePosition))
                {
                    isMouseOver = false;
                    control.IsMouseOver = false;
                }
            }

            // we now start over with a full screenpick. this is not the most efficient way, but it is a lot simpler.
            controlChain.Clear();
            ScreenPickInternal(controlChain, mousePosition);

            foreach (var control in controlChain)
            {
                control.IsMouseOver = true;
            }
        }

        protected void DispatchMouseMoveEvent(Vector2I previousPosition, Vector2I position)
        {
            if (_mouseCapturer != null)
                _mouseCapturer.OnMouseMoveInternal(previousPosition, position);
            else
            {
                foreach (var control in _mouseHoverChain)
                    control.OnMouseMoveInternal(previousPosition, position);
            }
        }

        private void DispatchMouseButtonChange(Vector2I position, MouseButton button, ButtonState state)
        {
            if (_mouseCapturer != null)
            {
                _mouseCapturer.OnMouseButtonChangeInternal(position, button, state);
            }
            else
            {
                foreach (var control in _mouseHoverChain)
                    control.OnMouseButtonChangeInternal(position, button, state);
            }
        }

        private void ProcessTypeInput(char ch, Platform.Inputs.Button button)
        {
            _focussedControl?.OnTypeInput(ch, button);
        }

        private void ProcessKeyStroke(KeyStrokeEvent e)
        {
            _focussedControl?.OnKeyStroke(e);
        }

        private void ProcessKeyDown(Platform.Inputs.Button e)
        {
            _focussedControl?.OnKeyDown(e);
        }

        private void ProcessKeyUp(Platform.Inputs.Button e)
        {
            _focussedControl?.OnKeyUp(e);
        }

        public void Draw(IImmediateRenderer renderer)
        {
            renderer.Begin(_camera, false, false, false);
            DoRender(renderer);
            renderer.End();
        }

        public void SetFocus(UIControl? control)
        {
            if (_focussedControl == control) return;

            if (_focussedControl != null) _focussedControl.HasFocus = false;
            _focussedControl = control;
            if (_focussedControl != null) _focussedControl.HasFocus = true;
        }


        public void CaptureMouse(UIControl control)
        {
            _mouseCapturer = control;
        }

        public void ReleaseMouse()
        {
            _mouseCapturer = null;
        }


        public Font DefaultFont { get; }

        public Color DefaultTextColor { get; set; } = PaletteApollo.Gray8;
        public Clock Clock { get; }
        public IClipboard Clipboard { get; }

        /// <summary>
        /// Allows parts of the UI to communicate.
        /// </summary>
        public MessageBus MessageBus { get; }
    }
}
