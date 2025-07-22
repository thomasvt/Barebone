using System.Drawing;

namespace BareBone.Animations
{
    public class ColorAnimation : Animation<Color>
    {

        protected override Color GetValue(float factor)
        {
            return Color.FromArgb(
                (byte)float.Lerp(From.A, To.A, factor),
                (byte)float.Lerp(From.R, To.R, factor),
                (byte)float.Lerp(From.G, To.G, factor),
                (byte)float.Lerp(From.B, To.B, factor)
            );
        }

        public Color From { get; set; }

        public Color To { get; set; }
    }
}
