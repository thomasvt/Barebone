namespace BareBone.Animations
{
    public class FloatAnimation : Animation<float>
    {

        protected override float GetValue(float factor)
        {
            return From * (1 - factor) + To * factor;
        }

        public float From { get; set; }

        public float To { get; set; }
    }
}
