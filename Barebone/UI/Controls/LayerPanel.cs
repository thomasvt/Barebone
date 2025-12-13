
namespace Barebone.UI.Controls
{
    /// <summary>
    /// A panel that stacks its children in fully docked layers. Children added later are rendered on top of earlier children.
    /// </summary>
    public class LayerPanel(UserInterface ui) : UIControl(ui)
    {
        public void AddChild(UIControl child)
        {
            Children.Add(child);
        }
    }
}
