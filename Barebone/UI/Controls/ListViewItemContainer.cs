using System.Drawing;

namespace Barebone.UI.Controls
{
    public class ListViewItemContainer : ButtonBase
    {
        public ListViewItemContainer(ListView listView, ListViewItem item) : base(listView.UI)
        {
            Item = item;
            BackgroundColorNormal = Color.Transparent;
            BackgroundColorHover = PaletteApollo.Pink1;
            BackgroundColorPressed = PaletteApollo.Pink2;

            var textBlock = new TextBlock(listView.UI) { Text = item.Label, VerticalAlignment = VerticalAlignment.Center, TextColor = item.Color ?? listView.UI.DefaultTextColor };
            Children.Add(textBlock);
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
            get;
            private set
            {
                if (field == value) return;
                field = value;
                UpdateStyle();
            }
        }
        public ListViewItem Item { get; }
    }
}
