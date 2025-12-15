using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.UI.Text;

namespace Barebone.UI.Controls
{
    public class ListView : UIControl
    {
        private readonly List<ListViewItem> _items = new();
        private readonly BBList<GpuTexTriangle> _textTriangles = new();
        private Font _font;
        private readonly StackPanel _stackPanel;
        private ListViewItemContainer? _selectedContainer;

        public IReadOnlyList<ListViewItem> Items => _items;

        public ListView(UserInterface ui) : base(ui)
        {
            IsFocussable = true;
            Indent = 12;
            Font = ui.DefaultFont;

            Children.Add(_stackPanel = new StackPanel(ui) { Orientation = Orientation.Vertical, DefaultItemSize = ui.DefaultFont.LineHeight + 8 });
        }

        public ListViewItem AddItem(string label, object? userData = null, Color? color = null)
        {
            var item = new ListViewItem(this, label, userData, color);
            _items.Add(item);

            var container = new ListViewItemContainer(this, item);
            container.Click = () => Select(container);
            _stackPanel.AddItem(container);

            InvalidateArrange();
            return item;
        }

        public void Clear()
        {
            _items.Clear();
            _stackPanel.Clear();
        }
        
        public void Select(ListViewItem item)
        {
            foreach (var child in _stackPanel.Children)
            {
                if (child is ListViewItemContainer container && container.Item == item) 
                    Select(container);
            }
        }

        private void Select(ListViewItemContainer? container)
        {
            if (container == _selectedContainer) return;

            _selectedContainer?.Deselect();
            _selectedContainer = container;
            _selectedContainer?.Select();
            if (_selectedContainer != null)
                Selected?.Invoke(_selectedContainer.Item);
        }

        protected override void Render(IImmediateRenderer renderer)
        {
            base.Render(renderer);
            renderer.Draw(Matrix4x4.Identity, _textTriangles.AsReadOnlySpan(), _font.Texture);
        }

        public Font Font
        {
            get => _font;
            set
            {
                if (_font == value) return;

                _font = value;
                InvalidateVisual();
            }
        }

        public Color TextColor
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
                InvalidateVisual();
            }
        }

        public int Indent
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
                InvalidateVisual();
            }
        }

        public event Action<ListViewItem>? Selected;
    }
}
