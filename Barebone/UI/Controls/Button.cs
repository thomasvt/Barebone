using System.Drawing;

namespace Barebone.UI.Controls
{
    public class Button : ButtonBase
    {
        private readonly TextBlock _textBlock;

        public Button(UserInterface ui) : base(ui)
        {
            _textBlock = new TextBlock(ui)
            {
                HorizontalAlignment = HorizontalAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center,
            };
            Children.Add(_textBlock);
        }

        public string Text
        {
            get => _textBlock.Text;
            set => _textBlock.Text = value;
        }

        public Color TextColor
        {
            get => _textBlock.TextColor;
            set => _textBlock.TextColor = value;
        }
    }
}
