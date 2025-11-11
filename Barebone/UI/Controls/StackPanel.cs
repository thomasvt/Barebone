using Barebone.Geometry;

namespace Barebone.UI.Controls
{
    public class StackPanel : UIControl
    {
        private int _itemSize;
        private Orientation _orientation;

        public StackPanel(UserInterface ui) : base(ui)
        {
            IsMouseInteractive = false;
            ItemSize = 20;
        }

        protected override void Arrange()
        {
            var cursor = Viewport.MinCorner;

            if (Orientation == Orientation.Vertical)
            {
                foreach (var child in Children)
                {
                    child.Viewport = new AabbI(cursor, cursor + new Vector2I(Viewport.Width, ItemSize));
                    cursor.Y += ItemSize;
                }
            }
            else
            {
                foreach (var child in Children)
                {
                    child.Viewport = new AabbI(cursor, cursor + new Vector2I(ItemSize, Viewport.Height));
                    cursor.X += ItemSize;
                }
            }
        }

        public void Clear()
        {
            Children.Clear();
            InvalidateArrange();
        }

        public void AddChild(UIControl control)
        {
            Children.Add(control);
            InvalidateArrange();
        }

        public int ItemSize
        {
            get => _itemSize;
            set
            {
                if (_itemSize == value) return;

                _itemSize = value;
                InvalidateArrange();
            }
        }

        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value) return;
                _orientation = value;
                InvalidateArrange();
            }
        }
    }
}
