using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.UI.Text;

namespace Barebone.UI.Controls
{
    public class TextBlock : UIControl
    {
        private readonly BBList<GpuTexTriangle> _textTriangles = new();
        private Font _font;

        public TextBlock(UserInterface ui) : base(ui)
        {
            Font = ui.DefaultFont;
            TextColor = ui.DefaultTextColor;
            IsHitTestEnabled = false;
        }

        protected override void Draw()
        {
            base.Draw();
            DrawText();
        }

        private void DrawText()
        {
            _textTriangles.Clear();

            if (string.IsNullOrWhiteSpace(Text)) return;

            var maxLinesVisible = Viewport.Height / _font.LineHeight + 1;
            var textSize = _font.Measure(Text);

            var textOffsetX = HorizontalAlignment switch
            {
                HorizontalAlignment.Left => 0,
                HorizontalAlignment.Right => Viewport.Size.X - textSize.X,
                HorizontalAlignment.Center => (Viewport.Size.X - textSize.X) / 2
            };

            var textOffsetY = VerticalAlignment switch
            {
                VerticalAlignment.Top => 0,
                VerticalAlignment.Bottom => Viewport.Size.Y - textSize.Y,
                VerticalAlignment.Center => (Viewport.Size.Y - textSize.Y) / 2
            };

            var topLeft = Viewport.MinCorner + new Vector2I(textOffsetX, textOffsetY);

            if (maxLinesVisible <= 0)
                return;

            _font.AppendString(true, _textTriangles, Text, TextColor, topLeft);
        }

        protected override void Render(IImmediateRenderer renderer)
        {
            base.Render(renderer);
            renderer.Draw(Matrix4x4.Identity, _textTriangles.AsReadOnlySpan(), _font.Texture);
        }

        public override void Dispose()
        {
            _textTriangles.Return();
            base.Dispose();
        }

        public string Text
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
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

        public HorizontalAlignment HorizontalAlignment
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateVisual();
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                InvalidateVisual();
            }
        }
    }
}
