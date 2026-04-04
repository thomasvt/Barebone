using Barebone.Geometry;

namespace Barebone.UI.Controls
{
    public enum DockType
    {
        Left, Top, Right, Bottom
    }

    internal static class LayoutExtensions
    {
        // mind that AabbI is designed for Y+ up, while we use it for a UI here, which is Y+ down. So using their Top and Bottom properties is avoided.

        public static void Dock(this AabbI area, DockType dockType, int size, int gap, out AabbI dockArea, out AabbI remainder)
        {
            switch (dockType)
            {
                case DockType.Left:
                    area.DockLeft(size, gap, out dockArea, out remainder);
                    break;
                case DockType.Top:
                    area.DockTop(size, gap, out dockArea, out remainder);
                    break;
                case DockType.Right:
                    area.DockRight(size, gap, out dockArea, out remainder);
                    break;
                case DockType.Bottom:
                    area.DockBottom(size, gap, out dockArea, out remainder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dockType));
            }
        }

        /// <returns>The remaining area for fluent API chaining</returns>
        public static void DockLeft(this AabbI area, int width, int gap, out AabbI dock, out AabbI remainder)
        {
            width = Math.Min(width, area.Width);
            dock = new AabbI(area.MinCorner.X, area.MinCorner.Y, area.MinCorner.X + width, area.MaxCorner.Y);
            if (dock.Width < 0) dock = AabbI.Zero;
            remainder = new AabbI(area.MinCorner.X + width + gap, area.MinCorner.Y, area.MaxCorner.X, area.MaxCorner.Y);
            if (remainder.Width < 0) remainder = AabbI.Zero;
        }

        /// <returns>The remaining area for fluent API chaining</returns>
        public static void DockTop(this AabbI area, int height, int gap, out AabbI dock, out AabbI remainder)
        {
            height = Math.Min(height, area.Height);
            dock = new AabbI(area.MinCorner.X, area.MinCorner.Y, area.MaxCorner.X, area.MinCorner.Y + height);
            if (dock.Height < 0) dock = AabbI.Zero;
            remainder = new AabbI(area.MinCorner.X, area.MinCorner.Y + height + gap, area.MaxCorner.X, area.MaxCorner.Y);
            if (remainder.Height < 0) remainder = AabbI.Zero;
        }

        /// <returns>The remaining area for fluent API chaining</returns>
        public static void DockRight(this AabbI area, int width, int gap, out AabbI dock, out AabbI remainder)
        {
            width = Math.Min(width, area.Width);
            dock = new AabbI(area.MaxCorner.X - width, area.MinCorner.Y, area.MaxCorner.X, area.MaxCorner.Y);
            if (dock.Height < 0) dock = AabbI.Zero;
            remainder = new AabbI(area.MinCorner.X, area.MinCorner.Y, area.MaxCorner.X - width - gap, area.MaxCorner.Y);
            if (remainder.Width < 0) remainder = AabbI.Zero;
        }

        /// <returns>The remaining area for fluent API chaining</returns>
        public static void DockBottom(this AabbI area, int height, int gap, out AabbI dock, out AabbI remainder)
        {
            height = Math.Min(height, area.Height);
            dock = new AabbI(area.MinCorner.X, area.MaxCorner.Y - height, area.MaxCorner.X, area.MaxCorner.Y);
            if (dock.Height < 0) dock = AabbI.Zero;
            remainder = new AabbI(area.MinCorner.X, area.MinCorner.Y, area.MaxCorner.X, area.MaxCorner.Y - height - gap);
            if (remainder.Height < 0) remainder = AabbI.Zero;
        }
    }
}
