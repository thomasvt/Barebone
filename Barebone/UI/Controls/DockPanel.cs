namespace Barebone.UI.Controls
{
    public class DockPanel(UserInterface ui) : UIControl(ui)
    {
        private record DockItem(DockType Side, int Size, int Gap, UIControl? Control);

        private readonly List<DockItem> _dockItems = new();
        private UIControl? _lastChild;

        protected override void Arrange()
        {
            var remainder = Viewport;
            foreach (var item in _dockItems)
            {
                remainder.Dock(item.Side, item.Size, item.Gap, out var dockArea, out remainder);
                if (item.Control != null) item.Control.Viewport = dockArea;
            }

            if (_lastChild != null)
                _lastChild.Viewport = remainder;
        }

        /// <summary>
        /// Docks a new child at a certain side of the parent.
        /// </summary>
        public void AddItem(DockType side, int size, int gap, UIControl control)
        {
            Children.Add(control);
            _dockItems.Add(new DockItem(side, size, gap, control));
        }

        /// <summary>
        /// Docks an amount of empty space.
        /// </summary>
        public void AddGap(DockType side, int size)
        {
            _dockItems.Add(new DockItem(side, size, 0, null));
        }

        public void AddLastChild(UIControl control)
        {
            if (_lastChild != null) throw new Exception("Can only have one 'last child' that fills the remaining area.");
            _lastChild = control;
            Children.Add(control);
        }
    }
}
