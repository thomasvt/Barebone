using Barebone.Geometry;

namespace Barebone.UI.Controls
{
    public class StackPanel : UIControl
    {
        private readonly List<StackLayoutInfo> _layoutInfos = new();

        public StackPanel(UserInterface ui) : base(ui)
        {
            DefaultItemSize = 20;
        }

        private record StackLayoutInfo(int Size, UIControl Control);

        protected override void Arrange()
        {
            var cursor = Viewport.MinCorner;

            if (Orientation == Orientation.Vertical)
            {
                foreach (var info in _layoutInfos)
                {
                    var child = info.Control;
                    child.Viewport = new AabbI(cursor, cursor + new Vector2I(Viewport.Width, info.Size));
                    cursor.Y += info.Size;
                }
            }
            else
            {
                foreach (var info in _layoutInfos)
                {
                    var child = info.Control;
                    child.Viewport = new AabbI(cursor, cursor + new Vector2I(info.Size, Viewport.Height));
                    cursor.X += info.Size;
                }
            }
        }

        public void Clear()
        {
            _layoutInfos.Clear();
            Children.Clear();
            InvalidateArrange();
        }

        public void AddItem(UIControl control, int? size = null)
        {
            size ??= DefaultItemSize;
            _layoutInfos.Add(new StackLayoutInfo(size.Value, control));
            Children.Add(control);
            InvalidateArrange();
        }

        public Orientation Orientation
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateArrange();
            }
        }
        public int DefaultItemSize { get; set; }
    }
}
