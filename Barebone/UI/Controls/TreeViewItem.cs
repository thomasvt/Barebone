using System.Drawing;

namespace Barebone.UI.Controls
{
    public record TreeViewItem(TreeView TreeView, string Label, Color? Color = null)
    {
        private readonly List<TreeViewItem> _items = new();
        private bool _isExpanded;

        public TreeViewItem AddChild(string label, Color? color)
        {
            var item = new TreeViewItem(TreeView, label, color);
            _items.Add(item);
            TreeView.InvalidateArrange();
            return item;
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                TreeView.InvalidateArrange();
            }
        }

        public IReadOnlyList<TreeViewItem> Items => _items;
    }
}
