using System.Drawing;
using Barebone.Geometry;

namespace Barebone.UI.Controls
{
    /// <summary>
    /// Button base interaction logic.
    /// </summary>
    public abstract class ButtonBase : UIControl
    {
        private bool _isMouseButtonDown;
        public Action? Click;
        private Color _backgroundColorNormal;
        private Color _backgroundColorHover;
        private Color _backgroundColorPressed;

        protected ButtonBase(UserInterface ui) : base(ui)
        {
            BackgroundColorNormal = PaletteApollo.Pink1;
            BackgroundColorHover = PaletteApollo.Pink2;
            BackgroundColorPressed = PaletteApollo.Pink3;

            UpdateStyle();
        }

        public bool IsMouseButtonDown
        {
            get => _isMouseButtonDown;
            set
            {
                if (_isMouseButtonDown == value) return;

                _isMouseButtonDown = value;
                UpdateStyle();
            }
        }

        protected override void OnMouseButtonChange(Vector2I position, MouseButton button, ButtonState state)
        {
            if (button == MouseButton.Left && state == ButtonState.Pressed)
            {
                IsMouseButtonDown = true;
                UI.CaptureMouse(this);
            }

            if (button == MouseButton.Left && state == ButtonState.Released)
            {
                IsMouseButtonDown = false;
                if (Viewport.Contains(position)) // only 'click' if the mouse is still over the button.
                {
                    Click?.Invoke();
                }
                UI.ReleaseMouse();
            }
        }

        protected virtual void UpdateStyle()
        {
            if (IsMouseButtonDown)
            {
                BackgroundColor = BackgroundColorPressed;
            }
            else
            {
                BackgroundColor = IsMouseOver ? BackgroundColorHover : BackgroundColorNormal;
            }
        }

        protected override void OnMouseEnter()
        {
            UpdateStyle();
        }

        protected override void OnMouseLeave()
        {
            UpdateStyle();
        }

        public Color BackgroundColorNormal
        {
            get => _backgroundColorNormal;
            set
            {
                if (_backgroundColorNormal == value) return;
                _backgroundColorNormal = value;
                UpdateStyle();
            }
        }

        public Color BackgroundColorHover
        {
            get => _backgroundColorHover;
            set
            {
                if (_backgroundColorHover == value) return;
                _backgroundColorHover = value;
                UpdateStyle();
            }
        }
        public Color BackgroundColorPressed
        {
            get => _backgroundColorPressed;
            set
            {
                if (_backgroundColorPressed == value) return;
                _backgroundColorPressed = value;
                UpdateStyle();
            }
        }
    }
}
