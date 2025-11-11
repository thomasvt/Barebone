using System.Drawing;

namespace Barebone.UI.Controls
{
    public record ListViewItem(ListView ListView, string Label, object? UserData, Color? Color = null);
}
