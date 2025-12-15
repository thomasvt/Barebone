using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Input;
using Barebone.Pools;

namespace Barebone.UI.Controls
{
    public class UIControl
    {
        protected readonly Mesh Mesh = Pool.Rent<Mesh>();
        protected bool IsVisualInvalid { get; private set; }
        protected bool IsArrangeInvalid { get; private set; }
        protected internal readonly List<UIControl> Children = new();
        
        public readonly UserInterface UI;

        public UIControl(UserInterface ui)
        {
            UI = ui;
            IsHitTestEnabled = true;
        }

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

        internal void UpdateInternal()
        {
            Update();
            foreach (var control in Children)
            {
                control.UpdateInternal();
            }
        }

        protected internal virtual void Update()
        { }

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
        /// Recursively adds the control and its children to 'chain' if the given 'position' is inside this control's Viewport.
        /// </summary>
        protected void ScreenPickInternal(List<UIControl> chain, Vector2I position)
        {
            if (Viewport.Contains(position) && IsHitTestEnabled)
            {
                chain.Add(this);
                foreach (var child in Children)
                {
                    child.ScreenPickInternal(chain, position);
                }
            }
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
            get;
            internal set
            {
                if (field == value) return;

                field = value;
                if (IsMouseOver) OnMouseEnter();
                else OnMouseLeave();
            }
        }


        public bool IsFocussable { get; set; } = false;

        public bool HasFocus
        {
            get;
            internal set
            {
                if (field == value) return;
                field = value;
                OnFocusChanged(value);
            }
        }

        public Color BackgroundColor
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateVisual();
            }
        }

        public Color BorderColor
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateVisual();
            }
        }

        public Color BorderColorNormal
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateVisual();
            }
        }

        public Color BorderColorFocus
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateVisual();
            }
        }

        public int BorderThickness
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateVisual();
            }
        }

        public AabbI Viewport
        {
            get;
            set
            {
                if (field == value) return;

                if (value.Width < 0) value.MaxCornerExcl = value.MaxCornerExcl with { X = value.MinCorner.X };
                if (value.Height < 0) value.MaxCornerExcl = value.MaxCornerExcl with { Y = value.MinCorner.Y };

                field = value;
                InvalidateArrange();
            }
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"[{GetType().Name}] {Name}";
        }

        public bool IsHitTestEnabled { get; set; }

        
    }
}
