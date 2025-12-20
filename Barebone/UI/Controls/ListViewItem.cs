using System.Drawing;

namespace Barebone.UI.Controls
{
    public record ListViewItem(ListView ListView, UIControl Control, object? UserData);
}
