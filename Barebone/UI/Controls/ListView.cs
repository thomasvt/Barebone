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
        private readonly UserInterface _ui;
        private readonly List<ListViewItem> _items = new();
        private readonly BBList<GpuTexTriangle> _textTriangles = new();
        private Font _font;
        private Color _textColor;
        private int _indent;
        private readonly StackPanel _stackPanel;
        private ListViewItemContainer? _selectedContainer;

        public ListView(UserInterface ui) : base(ui)
        {
            IsFocussable = true;
            _ui = ui;
            Indent = 12;
            Font = ui.DefaultFont;

            Children.Add(_stackPanel = new StackPanel(ui) { Orientation = Orientation.Vertical, ItemSize = ui.DefaultFont.LineHeight + 8 });
        }

        public ListViewItem AddItem(string label, object? userData = null, Color? color = null)
        {
            var item = new ListViewItem(this, label, userData, color);
            _items.Add(item);
            return item;
        }

        protected override void Arrange()
        {
            _stackPanel.Clear();
            GenerateItemContainers(_items, Viewport.MinCorner);
            base.Arrange();
        }

        private void GenerateItemContainers(IReadOnlyList<ListViewItem> items, Vector2I cursor)
        {
            foreach (var item in items)
            {
                var container = new ListViewItemContainer(this, item);
                container.Click = () => Select(container);

                _stackPanel.AddChild(container);

                cursor.Y += _stackPanel.ItemSize;
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
            get => _textColor;
            set
            {
                if (_textColor == value) return;

                _textColor = value;
                InvalidateVisual();
            }
        }

        public int Indent
        {
            get => _indent;
            set
            {
                if (_indent == value) return;

                _indent = value;
                InvalidateVisual();
            }
        }

        public event Action<ListViewItem>? Selected;
    }
}
