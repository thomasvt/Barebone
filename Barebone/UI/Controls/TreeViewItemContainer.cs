using System.Drawing;

namespace Barebone.UI.Controls
{
    public class TreeViewItemContainer : ButtonBase
    {
        public TreeViewItemContainer(UserInterface ui, TreeViewItem item, int indent) : base(ui)
        {
            Item = item;
            BackgroundColorNormal = Color.Transparent;
            BackgroundColorHover = PaletteApollo.Pink1;
            BackgroundColorPressed = PaletteApollo.Pink2;

            var dockPanel = new DockPanel(ui);

            Children.Add(dockPanel);

            if (item.Items.Count > 0)
            {
                var expandButton = new Button(ui)
                {
                    Text = item.IsExpanded ? "v" : ">",
                    BackgroundColorNormal = Color.Transparent,
                    BackgroundColorHover = Color.Transparent,
                    BackgroundColorPressed = Color.Transparent,
                };

                expandButton.Click = () =>
                {
                    if (ToggleExpand == null) return;
                    var isExpanded = ToggleExpand.Invoke();
                    expandButton.Text = isExpanded ? "v" : ">";
                };

                dockPanel.AddGap(DockSide.Left, indent);
                dockPanel.AddChild(DockSide.Left, 16, 8, expandButton);
            }
            else
            {
                dockPanel.AddGap(DockSide.Left, indent + 16 + 8);
            }

            dockPanel.AddlastChild(new TextBlock(ui) { Text = item.Label, TextColor = item.Color ?? ui.DefaultTextColor });
        }

        protected override void UpdateStyle()
        {
            if (IsSelected)
                BackgroundColor = BackgroundColorPressed;
            else
                base.UpdateStyle();
        }

        public void Deselect()
        {
            IsSelected = false;
        }

        public void Select()
        {
            IsSelected = true;
        }


        public bool IsSelected
        {
            get => _isSelected;
            private set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                UpdateStyle();
            }
        }
        public TreeViewItem Item { get; }

        internal Func<bool>? ToggleExpand;

        private bool _isSelected;
    }
}
