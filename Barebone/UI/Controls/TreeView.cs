using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.UI.Text;

namespace Barebone.UI.Controls
{
    public class TreeView : UIControl
    {
        private readonly UserInterface _ui;
        private readonly TreeViewItem _treeRootItem;
        private readonly BBList<GpuTexTriangle> _textTriangles = new();
        private Font _font;
        private readonly StackPanel _stackPanel;
        private TreeViewItemContainer? _selectedContainer;

        public TreeView(UserInterface ui) : base(ui)
        {
            IsFocussable = true;
            _ui = ui;
            _treeRootItem = new(this, string.Empty);
            Indent = 12;
            Font = ui.DefaultFont;

            Children.Add(_stackPanel = new StackPanel(ui) { Orientation = Orientation.Vertical, DefaultItemSize = 20 });
        }

        public TreeViewItem AddItem(string label, Color? color)
        {
            return _treeRootItem.AddChild(label, color);
        }

        protected override void Arrange()
        {
            _stackPanel.Clear();
            GenerateItemContainers(_treeRootItem.Items, Viewport.MinCorner, 8);
            base.Arrange();
        }

        private void GenerateItemContainers(IReadOnlyList<TreeViewItem> items, Vector2I cursor, int indentX)
        {
            foreach (var item in items)
            {
                var container = new TreeViewItemContainer(_ui, item, indentX)
                {
                    ToggleExpand = () =>
                    {
                        item.IsExpanded = !item.IsExpanded;
                        return item.IsExpanded;
                    }
                };
                container.Click = () => Select(container);

                _stackPanel.AddItem(container);

                cursor.Y += _font.LineHeight;
                if (item.IsExpanded && item.Items.Count > 0)
                {
                    GenerateItemContainers(item.Items, cursor, indentX + Indent);
                }
            }
        }

        private void Select(TreeViewItemContainer? container)
        {
            if (_selectedContainer == container) return;

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

        public event Action<TreeViewItem>? Selected;
    }
}
