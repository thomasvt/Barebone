using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Input;
using Barebone.Pools;

namespace Barebone.UI.Controls
{
    public class UIControl(UserInterface ui)
    {
        protected readonly Mesh Mesh = Pool.Rent<Mesh>();
        protected bool IsVisualInvalid { get; private set; }
        protected bool IsArrangeInvalid { get; private set; }
        protected readonly List<UIControl> Children = new();
        
        public readonly UserInterface UI = ui;
        private bool _hasFocus;
        private Color _backgroundColor;
        private Color _borderColor;
        private Color _borderColorFocus;
        private Color _borderColorNormal;
        private int _borderThickness;
        private bool _isMouseOver;
        private AabbI _viewport;
        private UIControl? _mouseOverControl;

        private void DoArrange()
        {
            IsVisualInvalid = true;
            Arrange();
        }

        /// <summary>
        /// Allows to arrange your children by setting their Viewport. Default is that all children will dock-fill their parent.
        /// </summary>
        protected virtual void Arrange()
        {
            DockFillAllChildren();
        }

        /// <summary>
        /// Utility method to arrange all children to fill the entire Viewport this parent.
        /// </summary>
        protected void DockFillAllChildren()
        {
            foreach (var child in Children)
            {
                child.Viewport = Viewport;
            }
        }

        internal void DoRender(IImmediateRenderer renderer)
        {
            if (IsArrangeInvalid)
            {
                DoArrange();
                IsArrangeInvalid = false;
            }

            renderer.PushClip(Viewport);

            if (IsVisualInvalid)
            {
                Draw();
                IsVisualInvalid = false;
            }

            Render(renderer);
            foreach (var child in Children)
            {
                child.DoRender(renderer);
            }
            renderer.PopClip();
        }

        protected virtual void Draw()
        {
            Mesh.Clear();
            var aabb = Viewport.ToAabb();
            if (BackgroundColor.A > 0)
                Mesh.FillAabbInZ(aabb, 0, BackgroundColor);
            if (BorderThickness > 0 && BorderColor.A > 0)
            {
                var hw = BorderThickness * 0.5f;
                Mesh.StrokeAabbInZ(aabb.Grow(-hw), hw, 0, BorderColor);
            }
        }

        protected virtual void Render(IImmediateRenderer renderer)
        {
            renderer.Draw(Matrix4x4.Identity, Mesh);
        }

        /// <summary>
        /// Drills down the part of the control hierarchy that is touched by `position`. Returns the deepest (leaf) control that is hit. Invokes 'visitAction' for each control passed.
        /// </summary>
        /// <param name="onlyMouseInteractive">Only considers controls that have IsMouseIntertive set to true.</param>
        /// <param name="visitAction">Invoked for each control passed during the drilldown. Passes the parent and child as arguments. Parameter 'onlyMouseInteractive' does not imfluence this being called.</param>
        protected UIControl? ScreenPick(Vector2I position, bool onlyMouseInteractive, Action<UIControl, UIControl>? visitAction = null)
        {
            foreach (var child in Children)
            {
                if (child.Viewport.Contains(position))
                {
                    visitAction?.Invoke(this, child);

                    var deepest = child.ScreenPick(position, onlyMouseInteractive, visitAction);
                    if (deepest != null)
                        return deepest;
                }
            }

            // hit is not on a child, so it's directly on this control:
            if (onlyMouseInteractive && !IsMouseInteractive)
                return null;
            return this;
        }

        public void SetFocus()
        {
            if (IsFocussable)
                UI.SetFocus(this);
        }

        internal void OnMouseButtonChangeInternal(Vector2I position, MouseButton button, ButtonState state)
        {
            if (button == MouseButton.Left && state == ButtonState.Pressed && IsFocussable)
                SetFocus();

            OnMouseButtonChange(position, button, state);
        }

        public void OnMouseMoveInternal(Vector2I previousPosition, Vector2I position)
        {
            OnMouseMove(previousPosition, position);
        }

        internal void MarkMouseOverChild(UIControl child)
        {
            if (_mouseOverControl != child)
            {
                _mouseOverControl?.ResetIsMouseOver();
                _mouseOverControl = child;
                _mouseOverControl.IsMouseOver = true;
            }
        }

        private void ResetIsMouseOver()
        {
            IsMouseOver = false;
            _mouseOverControl?.ResetIsMouseOver();
            _mouseOverControl = null;
        }

        protected virtual void OnMouseButtonChange(Vector2I position, MouseButton button, ButtonState state) { }
        protected virtual void OnMouseEnter() {}
        protected virtual void OnMouseLeave() {}
        public virtual void OnMouseMove(Vector2I previousPosition, Vector2I position) {}
        public virtual void OnTypeInput(char ch, Barebone.Platform.Inputs.Button button) { }
        public virtual void OnKeyStroke(KeyStrokeEvent e) { }
        public virtual void OnKeyDown(Barebone.Platform.Inputs.Button button) { }
        public virtual void OnKeyUp(Barebone.Platform.Inputs.Button button) { }

        protected virtual void OnFocusChanged(bool isFocussed)
        {
            if (IsFocussable)
            {
                if (isFocussed)
                {
                    BorderColor = BorderColorFocus;
                }
                else
                {
                    BorderColor = BorderColorNormal;
                }
            }
        }

        /// <summary>
        /// Forces the control to Draw() it's content again the next rendering frame.
        /// </summary>
        public void InvalidateVisual()
        {
            IsVisualInvalid = true;
        }

        /// <summary>
        /// Forces the control to Arrange() it's content again the next rendering frame. Also causes InvalidateVisual()
        /// </summary>
        public void InvalidateArrange()
        {
            IsArrangeInvalid = true;
            InvalidateVisual();
        }

        public virtual void Dispose()
        {
            foreach (var child in Children)
            {
                (child as IDisposable)?.Dispose();
            }
            Mesh.Return();
        }

        public bool IsMouseOver
        {
            get => _isMouseOver;
            internal set
            {
                if (_isMouseOver == value) return;

                _isMouseOver = value;
                if (IsMouseOver) OnMouseEnter();
                else OnMouseLeave();
            }
        }


        public bool IsFocussable { get; set; } = false;

        public bool HasFocus
        {
            get => _hasFocus;
            internal set
            {
                if (_hasFocus == value) return;
                _hasFocus = value;
                OnFocusChanged(value);
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor == value) return;
                _backgroundColor = value;
                InvalidateVisual();
            }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor == value) return;
                _borderColor = value;
                InvalidateVisual();
            }
        }
        
        public Color BorderColorNormal
        {
            get => _borderColorNormal;
            set
            {
                if (_borderColorNormal == value) return;
                _borderColorNormal = value;
                InvalidateVisual();
            }
        }

        public Color BorderColorFocus
        {
            get => _borderColorFocus;
            set
            {
                if (_borderColorFocus == value) return;
                _borderColorFocus = value;
                InvalidateVisual();
            }
        }

        public int BorderThickness
        {
            get => _borderThickness;
            set
            {
                if (_borderThickness == value) return;
                _borderThickness = value;
                InvalidateVisual();
            }
        }

        public AabbI Viewport
        {
            get => _viewport;
            set
            {
                if (_viewport == value) return;

                if (value.Width < 0) value.MaxCornerExcl = value.MaxCornerExcl with { X = value.MinCorner.X };
                if (value.Height < 0) value.MaxCornerExcl = value.MaxCornerExcl with { Y = value.MinCorner.Y };

                _viewport = value;
                InvalidateArrange();
            }
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"[{GetType().Name}] {Name}";
        }

        public bool IsMouseInteractive { get; set; } = true;

        
    }
}
